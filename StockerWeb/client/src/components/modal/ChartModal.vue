<template>
<modal
    class="w-modal"
    ref="chart_modal"
    id="chart-modal"
    name="chart-modal"
    :width="1000"
    :height="600"
    :isAutoHeight="false"
    :reset="false"
    :clickToClose="true"
    :resizable="false"
    @opened="openedModal"
    @closed="closedModal"
    draggable=".modal-header">
    <div class="modal-header">
        <h5>{{param.name}}</h5>
        <a class="close-modal-btn" role="button" @click="beforeModalClose()"><i class="el-icon-error"></i></a>
    </div>
    <div class="modal-body">
        <v-chart ref="echart" :options="chart_options" :initOptions="init_options"/>
    </div>
    <div class="modal-footer">
        <el-button size="mini" @click="onSetIchimoku()">일목균형표</el-button>
        <el-button size="mini" @click="onSetAlarm()">SET ALARM</el-button>
        <el-button size="mini" @click="onGotoChart()">GO TO CHART</el-button>
    </div>
</modal>
</template>

<script>

import echarts from 'echarts';
import moment from 'moment';
import api from '../../api/api.js'

export default {
    data () {
        return {
            param : {},
            init_options: {
                animation: false
            },
            chart_options:{
                animation: false
            }
        }
    },
    components:{
    },
    methods: {
        onSetAlarm() {
            var supstances = common.chart.getSupstances();
            var rawdata = {};
            supstances.map(function(v) {
                rawdata[v.type] = v.value
            })
            rawdata["name"] = this.param.name;
            var param = {
                category: this.param.id,
                email:sessionStorage.getItem('user'),
                rawdata:rawdata,
                favorite:true,
                alarm:true
            }
            api.setFavorite(param).then(function(){
                common.events.emit('message', {type:'success' , message:'저장 성공'})
            }).catch(function(err) {
                common.events.emit('message', {type:'fail' , message:'저장 실패'})
            })
        },
        show(d) {
            this.param = d;
            this.$modal.show('chart-modal');
        },
        onSetIchimoku() {
            common.chart.setIchimoku();
        },
        onGotoChart() {
            common.events.emit('onHandlePage', {page_name:'chart', params:this.param});
            this.$modal.hide('chart-modal');
        },
        beforeModalClose() {
            this.$modal.hide('chart-modal');
        },
        openedModal() {
            var me = this;
            //common.chart.init('chart-modal-space',{signal:true});
            api.getData(this.param.id, moment().add(1, 'day').format("YYYY-MM-DD")).then(function(data) {
                var golden_cross = [];
                var trades = [];
                var prev_datum;
                var buy_signal;
                var sell_signal;

                var box_range = {};
                var box_range2 = {}
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
                        if(d.total_state && moment().add(1,'day') >= new Date(d.unixtime)) {
                            var signal_count = 0;
                            if(prev_datum.current_state === '하락' && d.current_state === '상승') {
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
                                    box_range2 = d;
                                }
                                
                            }

                            if(prev_datum.current_state === '상승' && d.current_state === '하락') {
                                if(parseInt(d.props["최근갯수"]) < 3 && parseInt(d.props["과거갯수"]) > 2
                                && parseInt(d.props["최근갯수"]) <= parseInt(d.props["과거갯수"])) {
                                    box_range = d;
                                }
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
        closedModal() {
            common.events.emit('view.focus_target', this.param);
            this.param = {};
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
        console.log('mounted');
    },
    beforeUpdate() {

    },
    updated() {

    },
    beforeDestroy() {

    },
    destroyed() {
        console.log('destroyed')
    }
}
</script>
<style>


    body {
        font: 11px sans-serif;
    }

    text {
        fill: #000;
    }

    path.candle {
        stroke: #000000;
    }

    path.candle.body {
        stroke-width: 0;
    }

    path.candle.up {
        fill: #FF0000;
        stroke: #FF0000;
    }

    path.candle.down {
        fill: #0000FF;
        stroke: #0000FF;
    }

    path.ohlc {
        stroke: #000000;
        stroke-width: 1;
    }

    path.ohlc.up {
        stroke: #00AA00;
    }

    path.ohlc.down {
        stroke: #FF0000;
    }

    path.volume {
        fill: #DDDDDD;
    }

    path.line {
        fill: none;
        stroke: #BF5FFF;
        stroke-width: 1;
    }

    .extent {
        stroke: #fff;
        fill-opacity: .125;
        shape-rendering: crispEdges;
    }

    .crosshair {
        cursor: crosshair;
    }

    .crosshair path.wire {
        stroke: #DDDDDD;
        stroke-dasharray: 1, 1;
    }

    .crosshair .axisannotation path {
        fill: #DDDDDD;
    }


    .supstance path {
        stroke: black;
        stroke-width: 1;
        stroke-dasharray: 2, 2;
    }

    .scope-supstance.high path {
        stroke: darkblue;
        stroke-width: 2.5;
        stroke-dasharray: 3, 3;
    }

    .scope-supstance.support path {
        stroke: red;
        stroke-width: 1.5;
        stroke-dasharray: 4, 4;
    }

    .scope-supstance.regist path {
        stroke: blue;
        stroke-width: 1.5;
        stroke-dasharray: 4, 4;
    }

    .scope-supstance.loss path {
        stroke: darkred;
        stroke-width: 2.5;
        stroke-dasharray: 3, 3;
    }

    .scope-supstance.buy path {
        stroke: goldenrod;
        stroke-width: 2.5;
        stroke-dasharray: 5, 5;
    }

    .scope-supstance.sell path {
        stroke: violet;
        stroke-width: 2.5;
        stroke-dasharray: 5, 5;
    }

    .mouseover .supstance path {
        stroke-width: 1.5;
    }

    .dragging .supstance path {
        stroke: darkblue;
    }

    .axisannotation path {
        fill: black;
    }

    .axisannotation text {
        fill: #fff;
    }

    path.tradearrow {
        stroke: none;
    }
    .tradearrow path.highlight {
        fill: none;
        stroke-width: 2;
    }

    path.tradearrow.buy {
        fill: #FF0000;
    }
    .tradearrow path.highlight.buy {
        stroke: #FF0000;
    }

    path.tradearrow.buy-pending {
        stroke: green;
    }

    path.tradearrow.sell {
        fill: #9900FF;
    }
/* 
    .tradearrow path.highlight {
        fill: none;
        stroke-width: 2;
    }

    .tradearrow path.highlight.buy,.tradearrow path.highlight.buy-pending {
        stroke: #000000;
    }

    .tradearrow path.highlight.buy-pending {
        fill: #000000;
        fill-opacity: 0.3;
    }

    .tradearrow path.highlight.sell {
        stroke: #000000;
    } */

    .ichimoku path {
        fill: none;
        stroke-width: 0.8;
    }

    .ichimoku path {
        stroke: #000000;
    }

    .ichimoku path.chikouspan {
        stroke: #BF5FFF;
    }

    .ichimoku path.tenkansen {
        stroke: #0033FF;
    }

    .ichimoku path.kijunsen {
        stroke: #FBB117;
    }

    .ichimoku path.kumo {
        opacity: 0.1;
    }

    .ichimoku path.kumo.up {
        fill: #00AA00;
    }

    .ichimoku path.kumo.down {
        fill: #FF0000;
    }

    .ichimoku path.senkouspana {
        stroke: #006600;
    }

    .ichimoku path.senkouspanb {
        stroke: #FF0000;
    }

#chart-modal-space {
    width:100%;
    height:100%;
}

.area {
  fill: steelblue;
  clip-path: url(#clip);
}

.zoom {
  cursor: move;
  fill: none;
  pointer-events: all;
}
</style>