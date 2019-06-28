const _ = require('lodash');
const moment = require('moment');

common.chart = (function() {
    var isIchimoku = false;
    function canvasContextMenu() {
        common.events.emit('contextmenu', {
            active:true,
            left : d3.event.pageX,
            top : d3.event.pageY,
            params : {
                event:d3.event
            }
        });
        d3.event.stopPropagation();
        d3.event.preventDefault();
    }

    function canvasMouseDown() {
        common.events.emit('contextmenu', {
            active:false,
            x : d3.event.pageX,
            y : d3.event.pageY,
            params : {}
        });
    }

    function brushed() {
        if(d3.event) {
            var s = d3.event.selection || x2.range();
        }
        var zoomable = x.zoomable(),
            zoomable2 = x2.zoomable();

        zoomable.domain(zoomable2.domain());
        if(d3.event !== null && d3.event.selection !== null) zoomable.domain(d3.event.selection.map(zoomable.invert));
        
        draw();
    }

    function draw() {
        var candlestickSelection = focus.select("g.candlestick"),
            data = candlestickSelection.datum();
        y.domain(techan.scale.plot.ohlc(data.slice.apply(data, x.zoomable().domain()), candlestick.accessor()).domain());
        candlestickSelection.call(candlestick);

        focus.select("g.volume").call(volume);
        focus.select("g.x.axis").call(xAxis);
        focus.select("g.y.axis").call(yAxis);

        if(options.signal) {
            focus.selectAll("g.supstances").datum(supstanceData).call(supstance).call(supstance.drag).call(supstances_type);
            focus.selectAll("g.tradearrow").datum(trades).call(tradearrow);
        }

        var ichimokuData = isIchimoku ? ichimokuIndicator(data.slice.apply(data, x.zoomable().domain())) : [];
        // x.domain(data.map(ichimokuIndicator.accessor().d));
        // Calculate the y domain for visible data points (ensure to include Kijun Sen additional data offset)
        // y.domain(techan.scale.plot.ichimoku(ichimokuData.slice(indicatorPreRoll-ichimokuIndicator.kijunSen())).domain());
        // Logic to ensure that at least +KijunSen displacement is applied to display cloud plotted ahead of ohlc
        // x.zoomable().clamp(false).domain([indicatorPreRoll, data.length+ichimokuIndicator.kijunSen()]);
        focus.selectAll("g.ichimoku").datum(ichimokuData).call(ichimoku);
    }

    function load(data, end_date, supstance) {
        var accessor = candlestick.accessor(),
            timestart = Date.now();

        supstanceData = [];
        if(supstance) {
            // console.log(supstance);
            // _.each(supstance, function(v,i) {
            //     supstanceData.push({value:parseInt(v), type:'test'});
            // })
        }
        trades = [];
        var prev_datum;
        var buy_signal;
        var sell_signal;
        var buy_money = 0;
        var buy_volume = 0;
        var sell_money = 0;
        var sell_volume = 0;
        
        var end_date = end_date ? new Date(end_date) : new Date();
        data = data.map(function(d) {
            d.props = JSON.parse(d.props);
            if(d.props["지지가격대"]) {
                d.props["cross"] = d.props["지지가격대"].split(",");
            }

            if(moment(d.unixtime).format("YYYY-MM-DD") === moment(end_date).format("YYYY-MM-DD")) {
                if(d.props.cross) {
                    _.each(d.props.cross, function(cross,i) {
                        supstanceData.push({value:parseFloat(cross), type:'cross'});
                    })
                }
                supstanceData.push({value:Math.floor(d.props.last_support), type:'support'})
                supstanceData.push({value:Math.floor(d.props.last_resist), type:'regist'})
                
                var test = supstanceData.map(function(o) { return o.value })
                test.push(Math.floor(sell_signal.props.last_resist));
                test.push(Math.floor(buy_signal.props.last_support));
                // if(sell_signal) {
                //     supstanceData.push({value:Math.floor(sell_signal.props.last_resist), type:'high'})
                // }

                // if(buy_signal) {
                //     supstanceData.push({value:Math.floor(buy_signal.props.last_support), type:'loss'})
                // }
                var max = Math.max.apply(Math, test);
                var min = Math.min.apply(Math, test);
                _.each(trades, function(trade, k) {
                    if(trade.type.includes("buy")) {
                        if(trade.price >= min && trade.price <= max) {
                            buy_money += trade.price * trade.volume;
                            buy_volume += trade.volume
                        }
                    } else {
                        if(trade.price >= min && trade.price <= max) {
                            sell_money += trade.price * trade.volume;
                            sell_volume += trade.volume
                        }
                    }
                })
            }

            if(prev_datum) {
                if(d.total_state && moment(end_date).add(1,'day') >= new Date(d.unixtime)) {
                    if(prev_datum.current_state === '하락' && d.current_state === '상승' && parseInt(d.props["최근갯수"]) < 2) {
                        prev_buy_signal = buy_signal;
                        
                        // 하락 중 발생신호 탐색
                        if(parseFloat(d.props.resists) >= parseFloat(d.props.supports)) {
                            if(parseFloat(d.props.supports) < d.support_count) {
                                trades.push({date:parseDate(d.unixtime), type:'buy-pending', price:d.Low, volume:d.Volume, quantity:1});
                                buy_signal = d;
                            }
                        }
                        // 상승 중 발생신호 탐색
                        if(d.support_count <= d.regist_count) {
                            if(parseFloat(d.props.supports) > parseFloat(d.props.resists)) {
                                trades.push({date:parseDate(d.unixtime), type:'buy', price:d.Low, volume:d.Volume, quantity:1});
                                buy_signal = d;
                            }
                        }
                    }
                    
                    if(prev_datum.current_state === '상승' && d.current_state === '하락' && parseInt(d.props["최근갯수"]) < 2) {
                        prev_sell_signal = sell_signal;
                        if(parseFloat(d.props.supports) >= parseFloat(d.props.resists)) {
                            trades.push({date:parseDate(d.unixtime), type:'sell', price:d.High, volume:d.Volume, quantity:1});
                            sell_signal = d;
                        }
                    }
                }
            }
            prev_datum = d;
            
            return {
                date: parseDate(d.unixtime),
                open: d.Open === 0 ? d.Close : +d.Open,
                high: d.Open === 0 ? d.Close : +d.High,
                low: d.Open === 0 ? d.Close : +d.Low,
                close: d.Open === 0 ? d.Close : +d.Close,
                volume: d.Open === 0 ? d.Close : +d.Volume
            };
        }).sort(function(a, b) { return d3.ascending(accessor.d(a), accessor.d(b)); });

        supstanceData.push({value:buy_money / buy_volume, type:'buy'});
        supstanceData.push({value:sell_money / sell_volume, type:'sell'});
        

        x.domain(data.map(accessor.d));
        x2.domain(x.domain());
        y.domain(techan.scale.plot.ohlc(data, accessor).domain());
        y2.domain(y.domain());
        yVolume.domain(techan.scale.plot.volume(data).domain());

        focus.select("g.candlestick").datum(data);
        focus.select("g.volume").datum(data);

        context.select("g.close").datum(data).call(close);
        context.select("g.x.axis").call(xAxis2);

        // Associate the brush with the scale and render the brush only AFTER a domain has been applied
        context.select("g.pane").call(brush).selectAll("rect").attr("height", height2);

        x.zoomable().domain(x2.zoomable().domain());

        if(options.signal) {
            focus.append("g").attr("class", "supstances").attr("clip-path", "url(#supstanceClip)");
            focus.append("g").attr("class", "tradearrow").attr("clip-path", "url(#supstanceClip)");
        }

        focus.append('g').attr("class", "crosshair").call(crosshair);

        focus.append("g")
                .attr("class", "ichimoku")
                .attr("clip-path", "url(#supstanceClip)");

        draw();

        console.log("Render time: " + (Date.now()-timestart));
    }

    function supstances_type(node) {
        node.selectAll('.scope-supstance').each(function(d) {
            var supstance_node = d3.select(this);
            supstance_node.classed(d.type, true);
        })
    }

    function analysis(data, end_date, supstance) {
        var ret_data = {};
        ret_data["supstance"] = [];
        var end_date = end_date ? new Date(end_date) : new Date();
        var parseDate = d3.timeParse("%Y-%m-%dT%H:%M:%S.%LZ");

        var supstanceData = [];
        var trades = [];
        var prev_datum;
        var buy_signal;
        var sell_signal;
        var buy_money = 0;
        var buy_volume = 0;
        var sell_money = 0;
        var sell_volume = 0;
        data = data.map(function(d) {
            d.props = JSON.parse(d.props);
            if(d.props["지지가격대"]) {
                d.props["cross"] = d.props["지지가격대"].split(",");
            }

            if(moment(d.unixtime).format("YYYY-MM-DD") === moment(end_date).format("YYYY-MM-DD")) {
                if(d.props.cross) {
                    _.each(d.props.cross, function(cross,i) {
                        supstanceData.push({value:parseFloat(cross), type:'cross'});
                    })
                }
                supstanceData.push({value:Math.floor(d.props.last_support), type:'support'})
                supstanceData.push({value:Math.floor(d.props.last_resist), type:'regist'})
                
                var test = supstanceData.map(function(o) { return o.value })
                test.push(Math.floor(sell_signal.props.last_resist));
                test.push(Math.floor(buy_signal.props.last_support));
                var max = Math.max.apply(Math, test);
                var min = Math.min.apply(Math, test);
                _.each(trades, function(trade, k) {
                    if(trade.type.includes("buy")) {
                        if(trade.price >= min && trade.price <= max) {
                            buy_money += trade.price * trade.volume;
                            buy_volume += trade.volume
                        }
                    } else {
                        if(trade.price >= min && trade.price <= max) {
                            sell_money += trade.price * trade.volume;
                            sell_volume += trade.volume
                        }
                    }
                })
                var buy_price = buy_money / buy_volume;
                var sell_price = sell_money / sell_volume;
                if(!isNaN(buy_price)) ret_data["buy_price"] = buy_price;
                if(!isNaN(sell_price)) ret_data["sell_price"] = sell_price;
                ret_data["current"] = d;
                ret_data["prev_buy"] = buy_signal;
                ret_data["prev_sell"] = sell_signal;
            }

            if(prev_datum) {
                if(d.total_state && moment(end_date).add(1,'day') >= new Date(d.unixtime)) {
                    if(prev_datum.current_state === '하락' && d.current_state === '상승' && parseInt(d.props["최근갯수"]) < 2) {                        
                        // 하락 중 발생신호 탐색
                        if(parseFloat(d.props.resists) >= parseFloat(d.props.supports)) {
                            if(parseFloat(d.props.supports) < d.support_count) {
                                trades.push({date:parseDate(d.unixtime), type:'buy-pending', price:d.Low, volume:d.Volume, quantity:1});
                                buy_signal = d;
                            }
                        }
                        // 상승 중 발생신호 탐색
                        if(d.support_count <= d.regist_count) {
                            if(parseFloat(d.props.supports) > parseFloat(d.props.resists)) {
                                trades.push({date:parseDate(d.unixtime), type:'buy', price:d.Low, volume:d.Volume, quantity:1});
                                buy_signal = d;
                            }
                        }
                    }
                    
                    if(prev_datum.current_state === '상승' && d.current_state === '하락' && parseInt(d.props["최근갯수"]) < 2) {
                        prev_sell_signal = sell_signal;
                        if(parseFloat(d.props.supports) >= parseFloat(d.props.resists)) {
                            trades.push({date:parseDate(d.unixtime), type:'sell', price:d.High, volume:d.Volume, quantity:1});
                            sell_signal = d;
                        }
                    }
                }
            }
            prev_datum = d;
            
            return {
                date: parseDate(d.unixtime),
                open: d.Open === 0 ? d.Close : +d.Open,
                high: d.Open === 0 ? d.Close : +d.High,
                low: d.Open === 0 ? d.Close : +d.Low,
                close: d.Open === 0 ? d.Close : +d.Close,
                volume: d.Open === 0 ? d.Close : +d.Volume
            };
        });

        return ret_data;
    }

    return {
        analysis: analysis,
        setIchimoku: function () {
            isIchimoku = !isIchimoku;
            // var zoomable = x.zoomable(),
            // zoomable2 = x2.zoomable();

            // zoomable.domain(zoomable2.domain());
            draw();
        },
        getSupstances: function() {
            return supstanceData;
        },
        load: load,
        init:function(id, opt) {
            options = opt;
            container_div = document.getElementById(id);

            var margin_side = container_div.clientWidth/20 < 100 ? 100 : container_div.clientWidth/20;
            margin = {top: 20, right: margin_side/2, bottom: 100, left: margin_side};

            width = container_div.clientWidth - margin.left - margin.right,
            height = container_div.clientHeight - margin.top - margin.bottom;

            margin2 = {top: height + 20, right: margin.right, bottom: 20, left: margin.left}
            height2 = container_div.clientHeight - margin2.top - margin2.bottom;
            console.log(width,height,height2);

            parseDate = d3.timeParse("%Y-%m-%dT%H:%M:%S.%LZ");

            x = techan.scale.financetime().range([0, width]);
            y = d3.scaleLinear().range([height, 0]);

            yVolume = d3.scaleLinear().range([y(0), y(0.3)]);

            x2 = techan.scale.financetime().range([0, width]);
            y2 = d3.scaleLinear().range([height2, 0]);

            brush = d3.brushX().extent([[0, 0], [width, height2]])
                    .on("brush", brushed)
                    .on("end", brushed);

            candlestick = techan.plot.candlestick().xScale(x).yScale(y);

            ichimoku = techan.plot.ichimoku().xScale(x).yScale(y);

            ichimokuIndicator = techan.indicator.ichimoku();
            // Don't show where indicators don't have data
            indicatorPreRoll = ichimokuIndicator.kijunSen()+ichimokuIndicator.senkouSpanB();

            volume = techan.plot.volume().xScale(x).yScale(yVolume);

            close = techan.plot.close().xScale(x2).yScale(y2);

            xAxis = d3.axisBottom(x);
            yAxis = d3.axisLeft(y);

            xAxis2 = d3.axisBottom(x2);
            yAxis2 = d3.axisLeft(y2).ticks(0);

            ohlcAnnotation = techan.plot.axisannotation().axis(yAxis).orient('left').format(d3.format(',.2f'));

            timeAnnotation = techan.plot.axisannotation().axis(xAxis).orient('bottom').format(d3.timeFormat('%Y-%m-%d'))
                    .width(65).translate([0, height]);

            crosshair = techan.plot.crosshair().xScale(x).yScale(y).xAnnotation(timeAnnotation).yAnnotation(ohlcAnnotation);
            
            supstance = techan.plot.supstance().xScale(x).yScale(y).annotation([ohlcAnnotation]);

            tradearrow = techan.plot.tradearrow().xScale(x).yScale(y).orient(function(d) { return d.type.startsWith("buy") ? "up" : "down"; })

            outer = d3.select("#" + id)
            .append("svg:svg")
            .attr("width", width + margin.left + margin.right)
            .attr("height", height + margin.top + margin.bottom)
            .on('contextmenu', canvasContextMenu)
            .on('click', canvasMouseDown);

            defs = outer.append("defs");

            defs.append("clipPath")
            .attr("id", "ohlcClip")
            .append("rect")
            .attr("x", 0)
            .attr("y", 0)
            .attr("width", width)
            .attr("height", height);

            defs.append("clipPath")
            .attr("id", "supstanceClip")
            .append("rect")
            .attr("x", -margin.left)
            .attr("y", 0)
            .attr("width", width+margin.left)
            .attr("height", height);

            focus = outer.append("g")
            .attr("class", "focus")
            .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            focus.append("clipPath")
            .attr("id", "clip")
            .append("rect")
            .attr("x", 0)
            .attr("y", y(1))
            .attr("width", width)
            .attr("height", y(0) - y(1));

            focus.append("g")
            .attr("class", "volume")
            .attr("clip-path", "url(#clip)");

            focus.append("g")
            .attr("class", "candlestick")
            .attr("clip-path", "url(#clip)");

            focus.append("g")
            .attr("class", "x axis")
            .attr("transform", "translate(0," + height + ")");

            focus.append("g")
            .attr("class", "y axis")
            .append("text")
            .attr("transform", "rotate(-90)")
            .attr("y", 6)
            .attr("dy", ".71em")
            .style("text-anchor", "end");

            context = outer.append("g")
            .attr("class", "context")
            .attr("transform", "translate(" + margin2.left + "," + margin2.top + ")");

            context.append("g")
            .attr("class", "close");

            context.append("g")
            .attr("class", "pane");

            context.append("g")
            .attr("class", "x axis")
            .attr("transform", "translate(0," + height2 + ")");

            context.append("g")
            .attr("class", "y axis")
            .call(yAxis2);
        },
        uninit:function() {
            console.log(container_div);
            outer.remove();
        }
    }
})();