var _ = require('lodash');
var fs = require('fs');
var path = require('path');
var fsPath = require('fs-path');
var moment = require('moment');

const tf = require('@tensorflow/tfjs')
const nj = require('numjs');
require('@tensorflow/tfjs-node');
require('@tensorflow/tfjs-node-gpu');

const utils = require('../../utils/utils.js');

module.exports = {
    get : {
        "list" : function(req,res,next) {
            khan.model.current_stock.selectAll().then((data) => {
                res.status(200).send(data);
            }).catch((err) => {
                res.status(500).send(err);
            })
        },
        "daily" : function(req,res,next) {
            khan.model.past_stock.selectDaily().then((data) => {
                res.status(200).send(data);
            }).catch((err) => {
                res.status(500).send(err);
            })
        },
        "search" : function(req,res,next) {
            var session_user = req.session.passport.user._json.email;
            khan.model.current_stock.selectJoinFavorite(req.query.id, session_user).then((data) => {
                res.status(200).send(data);
            }).catch((err) => {
                res.status(500).send(err);
            })
        },
        "data" : function(req,res,next) {
            khan.model.past_stock.selectData(req.query.id, req.query.to_date, req.query.from_date).then((data) => {
                res.status(200).send(data);
            }).catch((err) => {
                res.status(500).send(err);
            })
        }
    },
    post: {
        "recommend" : function(req,res,next) {
            khan.model.past_stock.selectRecommend(req.body).then((data) => {
                var rows = data[0].map((d) => {
                    d.total_state = d.total_state.split(',');
                    d.current_state = d.current_state.split(',');
                    return d;
                })
                res.status(200).send(rows);
            }).catch((err) => {
                res.status(500).send(err);
            })
        },
        "recommends" : function(req,res,next) {
            var daylist = req.body;
            var promises = [];
            _.each(daylist, (param) => {
                promises.push(khan.model.past_stock.selectRecommends(param));
            })
            Promise.all(promises).then((result) => {
                res.status(200).send(result);
            }).catch((err) => {
                res.status(500).send(err);
            })
        },
        "predict": function(req,res,next) {
            new Promise((resolve, reject) => {
                var csv = req.body;
                var close = csv.data.map((el, idx) => {
                    return el[1];
                })
            
                var minmax_scaled = utils.minmax_1d(close)
                var timestamp = 20;
                var epoch = 32;
                var future = 60;
                var layer_size = 20;
                var learning_rate = 0.01;
                var smooth = 0.1;
                var X_scaled = minmax_scaled.scaled.slice([0], [Math.floor(minmax_scaled.scaled.shape[0]/timestamp)*timestamp+1])
            
                var cells = [tf.layers.lstmCell({units:layer_size})];
                var rnn = tf.layers.rnn({cell:cells, returnSequences: true, returnState : true});
                var dense_layer = tf.layers.dense({units:1, activation: 'linear'});
            
                var in_dropout_rate = 1;
                var out_dropout_rate = 0.99;
                function f(x,states) {
                    try {
                        x = utils.dropout_nn(x, in_dropout_rate);
                        var forward = rnn.apply(x, {initialState:states});
                        var last_sequences = utils.dropout_nn(forward[0].reshape([x.shape[1], layer_size]), out_dropout_rate);
                        return {'forward' : dense_layer.apply(last_sequences), 'state_1': forward[1], 'state_2':forward[2]};   
                    } catch (error) {
                        console.log(error);
                    }
                }
                var cost = (label, pred) => tf.square(tf.sub(label, pred)).mean();
                var optimizer = tf.train.adam(learning_rate);
                var batch_states = [tf.zeros([1, layer_size]), tf.zeros([1, layer_size])];
                var arr_loss = [];
            
                var learning_result = [];
            
                function async_training_loop(callback) {
                    (function loop(i) {
                        var total_loss = 0;
                        for(var k = 0; k < Math.floor(X_scaled.shape[0]/timestamp)*timestamp; k+=timestamp) {
                            batch_x = X_scaled.slice([k],[timestamp]).reshape([1,-1,1])
                            batch_y = X_scaled.slice([k+1],[timestamp]).reshape([-1,1])
                            feed = f(batch_x,batch_states)
                            optimizer.minimize(() => cost(batch_y,f(batch_x,batch_states)['forward']));
                            total_loss += parseFloat(cost(batch_y,f(batch_x,batch_states)['forward']).toString().slice(7));
                            batch_states = [feed.state_1,feed.state_2]
                        }
            
                        total_loss /= Math.floor(X_scaled.shape[0]/timestamp);
                        arr_loss.push(total_loss)
                        output_predict = nj.zeros([X_scaled.shape[0]+future, 1])
                        output_predict.slice([0,1],null).assign(utils.tf_str_tolist(X_scaled.slice(0,1))[0],false)
                        upper_b = Math.floor(X_scaled.shape[0]/timestamp)*timestamp
                        distance_upper_b = X_scaled.shape[0] - upper_b
                        batch_states = [tf.zeros([1,layer_size]),tf.zeros([1,layer_size])];
            
                        for(var k = 0; k < (Math.floor(X_scaled.shape[0]/timestamp)*timestamp); k+=timestamp){
                            batch_x = X_scaled.slice([k],[timestamp]).reshape([1,-1,1])
                            feed = f(batch_x,batch_states)
                            state_forward = utils.tf_nj_list(feed.forward)
                            output_predict.slice([k+1,k+1+timestamp],null).assign(state_forward,false)
                            batch_states = [feed.state_1,feed.state_2]
                        }
            
                        batch_x = X_scaled.slice([upper_b],[distance_upper_b]).reshape([1,-1,1])
                        feed = f(batch_x,batch_states)
                        state_forward = utils.tf_nj_list(feed.forward)
                        output_predict.slice([upper_b+1,X_scaled.shape[0]+1],null).assign(state_forward,false)
                        pointer = X_scaled.shape[0]+1
                        tensor_output_predict = output_predict.reshape([-1]).tolist()
                        batch_states = [feed.state_1,feed.state_2];
            
                        for(var k = 0; k < future-1; k+=1){
                            batch_x = tf.tensor(tensor_output_predict.slice(pointer-timestamp,pointer)).reshape([1,-1,1])
                            feed = f(batch_x,batch_states)
                            state_forward = utils.tf_nj_list(feed.forward.transpose())
                            tensor_output_predict[pointer] = state_forward[0][4]
                            pointer += 1
                            batch_states = [feed.state_1,feed.state_2]
                        }
                        console.log( 'Epoch: '+(i+1)+', avg loss: '+total_loss);
            
                        var predicted_val = utils.tf_nj_list_flatten(utils.reverse_minmax_1d(tf.tensor(tensor_output_predict),minmax_scaled['min'],minmax_scaled['max']));
                        var predicted_val = utils.smoothing_line(predicted_val,smooth);
                        console.log('Predict Result :', predicted_val.length);
                        learning_result.push({
                            id:"epoch"+(i+1),
                            loss:total_loss,
                            predicted: predicted_val
                        });
                        process.send({event:'predicted', data: predicted_val});
                        if(i < (epoch - 1)) {
                            loop(++i)
                        } else {
                            callback(predicted_val);
                        }
            
                    }(0))
                }
            
                async_training_loop(function(predicted) {
                    console.log('Done Training');
                    console.log(predicted[predicted.length-1] + '원');
                    var min_loss = learning_result.reduce(function(prev, curr) { return prev.loss < curr.loss ? prev : curr});
                    console.log(min_loss.predicted[min_loss.predicted.length-1] + '원');
                    process.send({event:'predicted', data: min_loss.predicted});
                })
            }).then(() => {
                console.log('real completed')
            })
            res.status(200).send()
        }
    }
}