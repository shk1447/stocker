var fs = require('fs');
var path = require('path');
var cmd = require('commander');

require('./server/utils/default_extend.js')

cmd.option('-m, --mode [mode]', 'set mode', 'production')
   .option('-c, --conf [conf]', 'set config', './config.json')
   .parse(process.argv);

var relative_path = path.relative(__dirname , './');
var config = JSON.parse(fs.readFileSync(path.resolve(cmd.conf), 'utf8'));

if(fs.existsSync(path.resolve('./google.json'))) {
    var auth_info = JSON.parse(fs.readFileSync(path.resolve('./google.json'), 'utf8'))
    config["google_auth"] = auth_info;
}

process.env.google_id = config.google_auth.id;
process.env.google_secret = config.google_auth.secret;

process.env.mode = cmd.mode;
process.env.port = config.port;
process.env.project = config.project.name;
process.env.database = config.database.type;
process.env.wsUrl = config.external.wsUrl;
process.env.httpUrl = config.external.httpUrl;
process.env.root_path = config.project.path !== "" ? path.resolve(config.project.path, './project/' + process.env.project + '/_build')
    : path.resolve(__dirname, relative_path, './project/' + process.env.project + '/_build');
require('./server')(config);