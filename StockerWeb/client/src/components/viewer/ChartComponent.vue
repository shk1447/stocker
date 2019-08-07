<template>
<div :class="collapsed ? 'content-wrapper' : 'content-wrapper show'" ref="content_wrapper">
    <div class="toolbar-wrapper">
        <div class="tool left">
            <el-autocomplete class="auto-input-custom" v-model="selected_item.name" value-key="name" :fetch-suggestions="querySearchAsync"
            placeholder="종목코드 및 종목명" @select="handleSelect">
                <i class="el-icon-search el-input__icon" slot="suffix"></i>
                <template slot-scope="{ item }">
                    <div style="display:flex;">
                        <div style="display:flex; flex:1 1 100%;">{{ item.name }}</div>
                        <div style="display:flex; flex:0 0 auto; padding:1em;"><i :class="item.favorite_type ? 'fas fa-star' : 'far fa-star'"></i></div>
                    </div>
                </template>
            </el-autocomplete>
        </div>
        <div style="flex:1 1 100%; "></div>
        <div class="tool right">
            <el-date-picker class="picker-custom"
            v-model="start_date"
            type="date"
            @change="onChangeDate"
            placeholder="주가 분석날짜 선택">
            </el-date-picker>
        </div>
        <div class="tool right">
            <span style="font-size:1.2em;" >
                ~
            </span>
        </div>
        <div class="tool right">
            <el-date-picker class="picker-custom"
            v-model="end_date"
            type="date"
            @change="onChangeDate"
            placeholder="주가 분석날짜 선택">
            </el-date-picker>
        </div>
        <div class="tool right">
            <span style="font-size:1.2em;" @click="onPredict">
                Predict
            </span>
        </div>
        <div class="tool right" @click="onAlarm">
            <span style="font-size:1.2em;">
                <i :class="alarm ? 'far fa-bell' : 'far fa-bell-slash'"></i>
            </span>
        </div>
        <div class="tool right" @click="onFavorite">
            <span style="font-size:1.2em;">
                <i :class="favorite ? 'fas fa-star' : 'far fa-star'"></i>
            </span>
        </div>
        <div class="tool right" @click="onSave">
            <!-- <el-badge is-dot class="item"> -->
                <span style="font-size:1.2em;">
                    <i class="fas fa-cloud-upload-alt"></i>
                    Save
                </span>
            <!-- </el-badge> -->
        </div>
        <div class="tool right" v-on:click="onFullScreen">
            <span style="font-size:1.2em;">
                <i class="fas fa-expand"></i>
            </span>
        </div>
    </div>
    <div id="sidebar" ref="sidebar"></div>
    <div id="sidebar-separator" class="ui-draggable"></div>
    <div id="chart-space" ref="chart_space" style="padding:30px;">
        <v-chart ref="echart" :options="chart_options" :initOptions="init_options"/>
    </div>
</div>
</template>

<script>

import * as tf from '@tensorflow/tfjs';
import moment from 'moment';
import api from '../../api/api.js';
import { setTimeout } from 'timers';
import echarts from 'echarts';

export default {
    data () {
        return {
            origin_data:{},
            init_options: {
                animation: false
            },
            chart_options:{
                animation: false
            },
            collapsed:true,
            open:true,
            selected_item:{
                category:"",
                name:""
            },
            signal:true,
            alarm:false,
            favorite:false,
            end_date:new Date(),
            start_date:moment().add('day', -730),
            data_type:'price',
            last_predicted:[]
        }
    },
    components:{
        
    },
    methods: {
        onPredict() {
            var me = this;
            api.executePredict(me.origin_data).then(function() {
                console.log('Predicting...')
            });            
        },
        onChangeDate() {
            this.refresh();
        },
        onSave() {
            if(this.selected_item.category) {
                var supstances = common.chart.getSupstances();
                var rawdata = {};
                supstances.map(function(v) {
                    rawdata[v.type] = v.value
                })
                rawdata["name"] = this.selected_item.name;
                var param = {
                    category: this.selected_item.category,
                    email:sessionStorage.getItem('user'),
                    rawdata:rawdata,
                    favorite:this.favorite,
                    alarm:this.alarm
                }
                console.log(supstances);
                api.setFavorite(param).then(function(){
                    common.events.emit('message', {type:'success' , message:'저장 성공'})
                }).catch(function(err) {
                    common.events.emit('message', {type:'fail' , message:'저장 실패'})
                })
                //this.alarm = !this.alarm;
            } else {
                common.events.emit('message', {type:'warning' , message:'검색 후 저장할 수 있습니다.'})
            }
        },
        onAlarm() {
            if(this.selected_item.category) {
                this.alarm = !this.alarm;
            } else {
                common.events.emit('message', {type:'warning' , message:'검색 후 설정가능합니다.'})
            }
        },
        onFavorite() {
            if(this.selected_item.category) {
                this.favorite = !this.favorite;
            } else {
                common.events.emit('message', {type:'warning' , message:'검색 후 설정가능합니다.'})
            }
        },
        onSignal() {
            this.signal = !this.signal;
        },
        handleSelect(item) {
            if(item.favorite_type) {
                this.favorite = true;
                this.alarm = item.favorite_type === 'alarm' ? true : false;
            } else {
                this.favorite = false;
                this.alarm = false;
            }
            this.selected_item = item;
            this.last_predicted = [];
            this.refresh();
        },
        querySearchAsync(queryString, cb) {
            if(queryString.length > 0) {
                api.getList(queryString).then(function(data) {
                    cb(data);
                })
            }
        },
        onFullScreen() {
            if(document.webkitIsFullScreen) {
                document.webkitCancelFullScreen();
            } else {
                document.documentElement.webkitRequestFullScreen();
            }
        },
        refresh() {
            var me = this;
            setTimeout(function() {
                var to_date = moment(me.end_date).add(1, 'day').format("YYYY-MM-DD")
                var from_date = moment(me.start_date).format("YYYY-MM-DD");
                api.getData(me.selected_item.category,to_date, from_date).then(function(data) {
                    //me.$refs.echart.options
                    // var supstance = []
                    // if(me.selected_item.supstance) {
                    //     supstance = me.selected_item.supstance.split(',');
                    // }
                    // common.chart.load(data, me.end_date, supstance);

                    var golden_cross = [];
                    var trades = [];
                    var prev_datum;
                    var buy_signal;
                    var sell_signal;

                    var box_range = {};
                    var box_range2 = {}
                    data = data.filter(function(d) { return moment(d.unixtime).format("YYYY-MM-DD") <= to_date });
                    var csv = {
                        volume : [],
                        data : [],
                        ma20 : [],
                        ma60 : [],
                        date : [],
                        markpoints : [],
                        markLines : [],
                        predicted:[],
                        last_resist:[],
                        last_support:[]
                    }

                    var temp_lastR = 0;
                    var temp_lastS = 0;
                    data.map(function(d,k) {
                        csv.volume.push(d.Volume);
                        csv.date.push(moment(d.unixtime).format('YYYY-MM-DD'));
                        csv.data.push([d.Open, d.Close, d.Low, d.High]);

                        d.props = JSON.parse(d.props);
                        if(d.props["지지가격대"]) {
                            d.props["cross"] = d.props["지지가격대"].split(",");
                        }

                        if(prev_datum) {
                            if(me.data_type === 'price') {
                                if(d.total_state && moment(me.end_date).add(1,'day') >= new Date(d.unixtime)) {
                                    var signal_count = 0;
                                    if(prev_datum.current_state === '하락' && d.current_state === '상승' && d.total_state === '상승') {
                                        var isSignal = false;
                                        if(parseInt(d.props["최근갯수"]) < 3 && parseInt(d.props["과거갯수"]) > 2
                                            && parseInt(d.props["최근갯수"]) <= parseInt(d.props["과거갯수"])) {
                                            var signal = {name:'buy', value:'buy', xAxis:k, yAxis:d.High,itemStyle:{color:'#61a0a8'}};
                                            if(parseFloat(prev_datum.props.last_resist) > prev_datum.Close && parseFloat(d.props.last_resist) < d.Close) {
                                                if(parseFloat(prev_datum.props.last_resist) - parseFloat(d.props.last_resist) > 0 && prev_datum.Close - d.Close < 0) {
                                                    signal_count++;
                                                }
                                            }
                                            if(prev_datum.support_count - d.support_count < 0 && prev_datum.regist_count - d.regist_count > 0
                                                && d.regist_count <= d.support_count) {
                                                signal_count++;
                                            }

                                            if(signal_count > 0) {
                                                if(signal_count > 1) {
                                                    signal.name = 'BUY';
                                                    signal.value = 'BUY';
                                                    signal.itemStyle.color = '#428688'
                                                }
                                                trades.push(signal);
                                                isSignal = true;
                                            }
                                        }
                                        if(!isSignal) box_range2 = d;
                                    }

                                    if(prev_datum.current_state === '상승' && d.current_state === '하락') {
                                        box_range = d;
                                    }
                                }
                            }

                            var last_resist = parseFloat(d.props.last_resist);
                            var last_support = parseFloat(d.props.last_support);
                            if(last_resist > 0) temp_lastR = last_resist;
                            else last_resist = temp_lastR;

                            if(last_support > 0) temp_lastS = last_support;
                            else last_support = temp_lastS;
                            
                            csv.last_resist.push([0,last_resist,0,0])
                            csv.last_support.push([0,last_support,0,0])
                        }
                        
                        prev_datum = d;
                    })

                    csv.last_resist = me.calculateMA(60, csv.last_resist);
                    csv.last_support = me.calculateMA(60, csv.last_support);
                    csv.ma20 = me.calculateMA(20, csv.data);
                    csv.ma60 = me.calculateMA(60, csv.data);
                    
                    csv.markpoints = trades;
                    csv.markLines.push({
                        name:'last_resist',
                        xAxis:csv.date[csv.date.length - 10],
                        yAxis:parseFloat(box_range.props.last_resist),
                        itemStyle: {
                            normal: {color: 'rgb(50,50,200)'}
                        }
                    })
                    csv.markLines.push({
                        name:'last_support',
                        xAxis:csv.date[csv.date.length - 10],
                        yAxis:parseFloat(box_range.props.last_support),
                        itemStyle: {
                            normal: {color: 'rgb(50,50,200)'}
                        }
                    })
                    csv.markLines.push({
                        name:'last_resist',
                        xAxis:csv.date[csv.date.length - 10],
                        yAxis:parseFloat(box_range2.props.last_resist),
                        itemStyle: {
                            normal: {color: 'rgb(200,50,50)'}
                        }
                    })
                    csv.markLines.push({
                        name:'last_support',
                        xAxis:csv.date[csv.date.length - 10],
                        yAxis:parseFloat(box_range2.props.last_support),
                        itemStyle: {
                            normal: {color: 'rgb(200,50,50)'}
                        }
                    })
                    csv.predicted = me.last_predicted;
                    me.origin_data = csv;
                    me.setOptions(csv);
                })
            },0)
        },
        calculateMA(dayCount, data) {
            var result = [];
            for (var i = 0, len = data.length; i < len; i++) {
                if (i < dayCount) {
                    result.push('-');
                    continue;
                }
                var sum = 0;
                for (var j = 0; j < dayCount; j++) {
                    sum += data[i - j][1];
                }
                result.push((sum / dayCount).toFixed(2));
            }
            return result;
        },
        setOptions(stocks) {
            var me = this;
            var color_list = ['#c23531','#2f4554', '#61a0a8', '#d48265', '#91c7ae','#749f83',  '#ca8622', '#bda29a','#6e7074', '#546570', '#c4ccd3'];
            // var dataMA5 = this.calculateMA(5, stocks.data);
            
            var option = {
                animation: false,
                color: color_list,
                title: {
                    left: 'center'
                },
                legend: {
                    top: 30,
                    data: ['STOCK', 'MA20', 'MA60', 'RESIST', 'SUPPORT', 'PREDICTED']
                },
                tooltip: {
                    trigger: 'axis',
                    axisPointer: {
                        animation: false,
                        type: 'cross'
                    }
                },
                axisPointer: {
                    link: [{
                        xAxisIndex: [0, 1]
                    }]
                },
                dataZoom: [{
                    type: 'slider',
                    xAxisIndex: [0, 1],
                    realtime: false,
                    start: 0,
                    end: 100,
                    top: 65,
                    height: 20,
                    handleIcon: 'M10.7,11.9H9.3c-4.9,0.3-8.8,4.4-8.8,9.4c0,5,3.9,9.1,8.8,9.4h1.3c4.9-0.3,8.8-4.4,8.8-9.4C19.5,16.3,15.6,12.2,10.7,11.9z M13.3,24.4H6.7V23h6.6V24.4z M13.3,19.6H6.7v-1.4h6.6V19.6z',
                    handleSize: '120%'
                    }, {
                    type: 'inside',
                    xAxisIndex: [0, 1],
                    start: 40,
                    end: 70,
                    top: 30,
                    height: 20
                }],
                xAxis: [{
                    type: 'category',
                    data: stocks.date,
                    boundaryGap : false,
                    axisLine: { lineStyle: { color: '#777' } },
                    axisLabel: {
                        formatter: function (value) {
                            return echarts.format.formatTime('MM-dd', value);
                        }
                    },
                    min: 'dataMin',
                    max: 'dataMax',
                    axisPointer: {
                        show: true
                    }
                    }, {
                    type: 'category',
                    gridIndex: 1,
                    data: stocks.date,
                    scale: true,
                    boundaryGap : false,
                    splitLine: {show: false},
                    axisLabel: {show: false},
                    axisTick: {show: false},
                    axisLine: { lineStyle: { color: '#777' } },
                    splitNumber: 20,
                    min: 'dataMin',
                    max: 'dataMax',
                    axisPointer: {
                        type: 'shadow',
                        label: {show: false},
                        triggerTooltip: true,
                        handle: {
                        show: true,
                        margin: 30,
                        color: '#B80C00'
                        }
                    }
                }],
                yAxis: [{
                        scale: true,
                        splitNumber: 2,
                        axisLine: { lineStyle: { color: '#777' } },
                        splitLine: { show: true },
                        axisTick: { show: false },
                        axisLabel: {
                            inside: true,
                            formatter: '{value}\n'
                        }
                        }, {
                        scale: true,
                        gridIndex: 1,
                        position:'right',
                        splitNumber: 2,
                        axisLabel: {show: false},
                        axisLine: {show: false},
                        axisTick: {show: false},
                        splitLine: {show: false}
                    }],
                grid: [{
                    left: 20,
                    right: 30,
                    top: 110,
                    }, {
                    left: 20,
                    right: 30,
                    top: 400
                }],
                graphic: [{
                    type: 'group',
                    left: 'center',
                    top: 70,
                    width: 300,
                    bounding: 'raw',
                    children: [{
                        id: 'MA5',
                        type: 'text',
                        style: {fill: color_list[1]},
                        left: 0
                    }, {
                        id: 'MA10',
                        type: 'text',
                        style: {fill: color_list[2]},
                        left: 'center'
                    }, {
                        id: 'MA20',
                        type: 'text',
                        style: {fill: color_list[3]},
                        right: 0
                    }]
                }],
                series: [{
                    name: 'Volume',
                    type: 'bar',
                    xAxisIndex: 1,
                    yAxisIndex: 1,
                    itemStyle: {
                        normal: {
                            color: '#7fbe9e'
                        }
                    },
                    data: stocks.volume
                }, {
                    type: 'candlestick',
                    name: 'STOCK',
                    data: stocks.data,
                    markPoint: {
                        data: stocks.markpoints
                    },
                    markLine: {
                        data: stocks.markLines
                    }
                }, {
                    name: 'MA20',
                    type: 'line',
                    data: stocks.ma20,
                    smooth: true,
                    showSymbol: false,
                    lineStyle: {
                        normal: {
                        width: 1
                        }
                    }
                },
                {
                    name: 'MA60',
                    type: 'line',
                    data: stocks.ma60,
                    smooth: true,
                    showSymbol: false,
                    lineStyle: {
                        normal: {
                        width: 1
                        }
                    }
                },{
                    name: 'RESIST',
                    type: 'line',
                    data: stocks.last_resist,
                    smooth: true,
                    showSymbol: false,
                    lineStyle: {
                        normal: {
                            width: 2
                        }
                    }
                },
                {
                    name: 'SUPPORT',
                    type: 'line',
                    data: stocks.last_support,
                    smooth: true,
                    showSymbol: false,
                    lineStyle: {
                        normal: {
                            width: 2
                        }
                    }
                },{
                    name: 'predicted',
                    type: 'line',
                    data: stocks.predicted,
                    smooth: false,
                    showSymbol: false,
                    lineStyle: {
                        normal: {
                            width: 2
                        }
                    }
                }]
            };
            me.chart_options = option;
        },
        onReceiveData(data) {
            var me = this;
            var predicted_series = this.chart_options.series.find(function(d) {
                return d.name === 'predicted'
            })
            var date = this.chart_options.xAxis[0].data;
            predicted_series.data = data;
            if(date.length < predicted_series.data.length) {
                var last_date = moment(date[date.length-1]);
                var current_index = date.length;
                for(var i = 0; i < predicted_series.data.length - current_index; i++) {
                    date.push(last_date.add(1,'day').format("YYYY-MM-DD"))
                }
            }
            me.last_predicted = data;
            //console.log(me.origin_data.)
            //console.log(this.chart_options.series);
        }
    },
    beforeCreate(){

    },
    created() {
        console.log('created')
    },
    beforeRouteUpdate(to,from){

    },
    mounted() {
        var me = this;
        console.log('mounted');
        common.socket.on('predicted', me.onReceiveData);
        me.$nextTick(function(){
            common.socket.emit('connected', "test");
        });

        var sidebar =  {};
        $('#sidebar-separator').draggable({
            axis:"x",
            start:function(event, ui) {
                sidebar.closing = false;
                sidebar.start = ui.position.left;
                sidebar.width = me.$refs.content_wrapper.clientWidth - ui.position.left - 12;
            },
            drag: function(event, ui) {
                var sidebar_width = me.$refs.content_wrapper.clientWidth - ui.position.left - 12;
                var chart_space_width = ui.position.left - 2;
                
                me.$refs.sidebar.style.width = sidebar_width + 'px';
                me.$refs.chart_space.style.width = chart_space_width + 'px';
                me.$refs.sidebar.style.border = 'none';
            
                if(sidebar_width < 200) {
                    sidebar.closing = true;
                } else {
                    sidebar.closing = false;
                }
                if(sidebar.closing) {
                    me.$refs.sidebar.style.border = '1px dashed rgb(80, 80, 80)';
                }
                
            },
            stop: function(event, ui) {
                if(sidebar.closing) {
                    var origin_width = me.$refs.content_wrapper.clientWidth - 12;
                    me.$refs.chart_space.style.width = origin_width + 'px';
                    me.$refs.sidebar.style.width = '0px';
                    me.$refs.sidebar.style.border = 'none';
                }
                $("#sidebar-separator").css("left","auto");
                $("#sidebar-separator").css("right",($("#sidebar").width())+"px");
                
                if(me.selected_item.category) me.refresh();
            }
        });
    },
    beforeUpdate() {

    },
    updated() {
        
    },
    beforeDestroy() {

    },
    destroyed() {
        common.socket.off('predicted', this.onReceiveData);
        console.log('destroyed')
    }
}
</script>
<style>
  
#chart-space {
    user-select: none;
    display: flex;
    height: calc(100% - 50px);
}

.toolbar-wrapper {
    display:flex; height:49px;width:100%;border-bottom:1px solid #e0e3eb;
}

.tool {
    display:flex; flex:0 0 auto; align-items:center; justify-content:center; padding:1.5em; cursor: pointer;
}
.tool:hover {
    background-color:#f0f3fa;   
}

.tool.left {
    border-right:1px solid #e0e3eb;
}

.tool.right {
    border-left:1px solid #e0e3eb;
}

.tool.signal {
    background-color: #cadeef;
}

.auto-input-custom>.el-input>input {
    border:none;
    background-color: transparent;
}

#sidebar-separator {
    position: absolute;
    top:50px !important;
    left:auto;
    right:0px;
    bottom:0px;
    width:10px;
    cursor: col-resize;
    z-index: 10;
    background: #f3f3f3;
    border-left:1px solid #e0e3eb;
    border-right:1px solid #e0e3eb;
}

#sidebar {
    position: absolute;
    top:50px !important;
    right:0px;
    bottom:0px;
    width:0px;
    background: #ffffff;
    z-index:11;
    box-sizing: border-box;
}

.echarts {
    width: 100% !important;
    height: 100% !important;
}

</style>