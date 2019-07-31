var instance;
const tf = require('@tensorflow/tfjs')
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
}

instance = instance ? instance : new Utils();
module.exports = instance;