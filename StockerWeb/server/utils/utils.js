var instance;
const tf = require('@tensorflow/tfjs')
const nj = require('numjs')
require('@tensorflow/tfjs-node');
require('@tensorflow/tfjs-node-gpu');

function Utils() {
    function tf_str_tolist(a){
        return JSON.parse(a.toString().slice(7).trim())
    }

    this.minmax_1d = function(a){
        a = tf.tensor(a)
        a_min = tf_str_tolist(a.min())
        a_max = tf_str_tolist(a.max())
        scaled = tf.div(tf.sub(a,a.min()), tf.sub(a.max(),a.min()))
        return {'scaled':scaled,'min':a_min,'max':a_max}
    }

    this.dropout_nn = function (x,keep_prob){
        uniform = tf.randomUniform(x.shape)
        added = tf.add(tf.scalar(keep_prob),uniform)
        binary = tf.floor(added)
        return tf.mul(tf.div(x,tf.scalar(keep_prob)),binary)
    }

    this.tf_nj_list = function (a){
        var arr = nj.zeros([a.shape[0],a.shape[1]]).tolist();
        for(var i = 0; i < a.shape[0];i++){
          for(var k = 0; k < a.shape[1];k++) arr[i][k] = JSON.parse(a.slice([i,k],[1,1]).toString().slice(7).trim().replace(',',''))[0][0]
        }
        return arr
    }

    this.tf_nj_list_flatten = function (a){
        var arr = nj.zeros([a.shape[0]]).tolist();
        for(var i = 0; i < a.shape[0];i++) arr[i] = JSON.parse(a.slice([i],[1]).toString().slice(7).trim())[0]
        return arr
    }

    this.tf_str_tolist = function (a){
        return JSON.parse(a.toString().slice(7).trim())
    }

    this.reverse_minmax_1d = function (a, a_min, a_max){
        return tf.add(tf.mul(a, tf.scalar(a_max-a_min)), tf.scalar(a_min))
      }

    this.smoothing_line = function (scalars,weight){
        var last = scalars[0]
        smoothed = []
        for(var i = 0; i < scalars.length;i++){
            smoothed_val = last * weight + (1 - weight) * scalars[i]
            smoothed.push(smoothed_val)
            last = smoothed_val
        }
        return smoothed
    }
}

instance = instance ? instance : new Utils();
module.exports = instance;