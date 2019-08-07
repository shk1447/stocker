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
        "test": function(req,res,next) {
            var promise_arr = [];
            _.each(req.body, (v,k) => {
                promise_arr.push(khan.model.past_stock.selectRecommend(v));
            })
            var test_data = {};
            Promise.all(promise_arr).then((dataset) => {
                _.each(dataset, (v,k) => {
                    var rows = v[0];
                    _.each(rows, (row, i) => {
                        if(!test_data[row.id]) {
                            test_data[row.id] = row;
                            test_data[row.id]["good_count"] = 1;
                        } else {
                            test_data[row.id]["good_count"]++;
                        }
                    })
                });
                console.log(Object.keys(test_data).length);
                var result = [];
                var flow_date = [];
                var promise = new Promise((resolve, reject) => {
                    _.each(Object.keys(test_data), (id, i) => {
                        var good_stock = test_data[id];
                        
                        khan.model.past_stock.selectData(id, moment().add('day', 1).format("YYYY-MM-DD"), undefined).then((data) => {
                            good_stock.props = JSON.parse(good_stock.props);
                            var start_date = moment();
                            _.each(good_stock.props, (value,key) => {
                                if(key.includes("_support_")) {
                                    var test_date = moment(key.split("_support_")[1]);
                                    if(start_date > test_date) {
                                        start_date = test_date
                                    }
                                } else if(key.includes("_resistance_")) {
                                    var test_date = moment(key.split("_resistance_")[1]);
                                    if(start_date > test_date) {
                                        start_date = test_date
                                    }
                                }
                            })
                            // console.log(good_stock.name, " : ", start_date.format("YYYY-MM-DD"));
                            var past_data = data.filter((a) => {
                                return moment(a.unixtime) < moment(good_stock.unixtime) && moment(a.unixtime) >= start_date;
                            });
                            var prev_data;
                            var buy_price = 0;
                            var buy_count = 0;
                            var sell_price = 0;
                            var sell_count = 0;
                            past_data.map((row) => {
                                row.props = JSON.parse(row.props);
                                if(prev_data) {
                                    if(prev_data.total_state === '하락' && row.total_state === '상승') {
                                        buy_price += row.Close;
                                        buy_count++;
                                    }

                                    if(prev_data.total_state === '상승' && row.total_state === '하락') {
                                        sell_price += row.Close;
                                        sell_count++;
                                    }
                                    if(good_stock.name === '코프라') {
                                        var last_resist = parseFloat(row.props.last_resist) > 0 ? parseFloat(row.props.last_resist) : row.Close;
                                        var last_support = parseFloat(row.props.last_support) > 0 ? parseFloat(row.props.last_support) : row.Close;
                                        console.log(moment(row.unixtime).format("YYYY-MM-DD") , ' : ', (last_resist + last_support)/2);
                                    }
                                }
                                prev_data = row;
                            })
                            //console.log(good_stock.name, " : ", total_price / signal_count, '원');
                            good_stock["buy_price"] = buy_price / buy_count;
                            good_stock["sell_price"] = sell_price / sell_count;
                            
                            var future_data = data.filter((a) => { return a.unixtime > good_stock.unixtime})
                            if(future_data.length > 0) {
                                var best_obj = future_data.reduce(function(prev, current) { return (prev.High > current.High) ? prev : current;});
                                var wow = (best_obj.High - good_stock.price) / good_stock.price * 100;
                                good_stock["yield"] = wow;
                            } else {
                                good_stock["yield"] = 0;
                            }
                            
                            result.push(good_stock);
                            if(result.length === Object.keys(test_data).length) {
                                resolve();
                            }
                        });
                        //console.log(id);
                    })
                })

                promise.then(() => {
                    var test = 0;
                    var sorted_arr = result.sort(function(prev, current) { return prev.yield < current.yield ? -1 : prev.yield > current.yield ? 1 : 0;});
                    _.each(sorted_arr, (v,k) => {
                        console.log(v.name,'(',moment(v.unixtime).format("YYYY-MM-DD"), '[',v.good_count, ']) : ', v.yield, '%,',
                        ' / 매수평균가 : ', v.buy_price, '/ 매도평균가 : ', v.sell_price, ' / 추천일 가격 : ', v.price, '/', parseFloat(v.props["V패턴_비율"]) - parseFloat(v.props["A패턴_비율"]));
                        if(v.yield > 10) {
                            test++;
                        }
                    })
                    console.log('10%이상 수익 중목 : ', test, ' 종목');
                    console.log(sorted_arr.length);
                })
            })
            res.status(200).send();
        },
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
                var close = csv.ma20.map((el, idx) => {
                    return el;
                })
            
                var minmax_scaled = utils.minmax_1d(close)
                var timestamp = 5;
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
                var out_dropout_rate = 0.95;
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