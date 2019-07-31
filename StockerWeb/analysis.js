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

khan.model.past_stock.selectByCategory('000020').map((row) => {
    row.rawdata = JSON.parse(row.rawdata);
    return row;
}).then((data) => {
    //console.log(data);
    console.log(tf);
})