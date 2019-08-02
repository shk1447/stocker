var _ = require('lodash');
var fs = require('fs');
var path = require('path');
var fsPath = require('fs-path');
var moment = require('moment');

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
            var csv = {
                volume : [],
                data : [],
                date : []
            }
            khan.model.past_stock.selectByCategory([{key:'category',condition:'=',value:req.query.id},{key:'unixtime',condition:'<=',value:req.query.to_date}])
            .map((row) => {
                row.rawdata = JSON.parse(row.rawdata);
                csv.volume.push(parseFloat(row.rawdata["거래량"]));
                csv.date.push(moment(row.unixtime).format('YYYY-MM-DD'));
                csv.data.push([parseFloat(row.rawdata["시가"]),
                parseFloat(row.rawdata["종가"]),
                parseFloat(row.rawdata["저가"]),
                parseFloat(row.rawdata["고가"])]);
                
                return row;
            }).then((data) => {
                res.status(200).send(csv);
            })
        },
        "analysis": function(req,res,next) {
            res.status(200).send()
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
        }
    }
}