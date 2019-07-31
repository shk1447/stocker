const _ = require('lodash');
const si = require('systeminformation');
const tf = require('@tensorflow/tfjs')
const knex = require('knex');
require('@tensorflow/tfjs-node');
require('@tensorflow/tfjs-node-gpu');

var fs = require('fs');
var path = require('path');
var cmd = require('commander');

cmd.option('-m, --mode [mode]', 'set mode', 'production')
   .option('-c, --conf [conf]', 'set config', './config.json')
   .parse(process.argv);

var config = JSON.parse(fs.readFileSync(path.resolve(cmd.conf), 'utf8'));

khan = {
    database:null,
    model:null
}

khan.database = knex({
    client:config.database.type,
    connection : config.database[config.database.type],
    pool: {min:0,max:10}
})
const model = require('./server/model');
khan.model = model();
_.each(khan.model, (d,i) => {
    d.initialize();
})

const moment = require('moment');
const fsPath = require('fs-path');
var csv = {
    date:[],
    volume:[],
    data:[]
}

var utils = require('./machine_learning/utils/utils.js')

// [{key:'category',condition:'=',value:'009150'},{key:'unixtime',condition:'<=',value:'2019-01-22'}]
khan.model.past_stock.selectByCategory([{key:'category',condition:'=',value:'085310'},{key:'unixtime',condition:'<=',value:'2019-06-18'}]).map((row) => {
    row.rawdata = JSON.parse(row.rawdata);
    csv.volume.push(parseFloat(row.rawdata["거래량"]));
    csv.date.push(moment(row.unixtime).format('YYYY-MM-DD'));
    csv.data.push([parseFloat(row.rawdata["시가"]),
    parseFloat(row.rawdata["종가"]),
    parseFloat(row.rawdata["고가"]),
    parseFloat(row.rawdata["저가"])]);
    
    return row;
}).then((data) => {
    var close = csv.data.map((el, idx) => {
        return el[1];
    })

    var stocks = csv.data.map((el, idx) => {
        return [el[0], el[1], el[3], el[2]];
    })
    var stock_date = csv.date;
    var volume = csv.volume;

    var test = utils.minmax_1d(close)
    tf.tensor3d([1, 2, 3, 4], [2, 2, 1]).print();

    fsPath.writeFile('./test.json', JSON.stringify(csv), 'utf-8', function(){
        console.log('write!!!')
    });
    
    // const model = tf.sequential();
    // model.add(tf.layers.dense({units:1, inputShape:[1]}));

    // model.compile({loss:'meanSquaredError', optimizer:'sgd'});

    // const xs = tf.tensor1d([1,2,3,4]);
    // const ys = tf.tensor1d([1,2,3,4]);

    // model.fit(xs, ys).then(() => {
    //     model.predict(tf.tensor2d([5], [1,1])).print();
    // })
})