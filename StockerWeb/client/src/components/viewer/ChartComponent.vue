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
            v-model="end_date"
            type="date"
            @change="onChangeDate"
            placeholder="주가 분석날짜 선택">
            </el-date-picker>
        </div>
        <div class="tool right">
            <span style="font-size:1.2em;" @click="onTrain">
                TRAIN
            </span>
        </div>
        <div class="tool right">
            <span style="font-size:1.2em;" @click="onPrice">
                PRICE
            </span>
        </div>
        <div class="tool right">
            <span style="font-size:1.2em;" @click="onResist">
                RESIST
            </span>
        </div>
        <div class="tool right">
            <span style="font-size:1.2em;" @click="onSupport">
                SUPPORT
            </span>
        </div>
        <div class="tool right">
            <span style="font-size:1.2em;" @click="onSetIchimoku">
                일목균형표
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
    <div id="chart-space" ref="chart_space">
        <v-chart ref="echart" :options="chart_options"/>
    </div>
</div>
</template>

<script>

import * as tf from '@tensorflow/tfjs';
import moment from 'moment';
import api from '../../api/api.js';
import { setTimeout } from 'timers';
import echarts from 'echarts';
// import { func } from '@tensorflow/tfjs-data';
// import { constants } from 'fs';

export default {
    data () {
        return {
            chart_options:{},
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
            data_type:'price'
        }
    },
    components:{
        
    },
    methods: {
        onTrain() {
            var data = common.chart.getTrades().filter(function(t) {
                return t.type === 'buy';
            });

            const values = data.map(function(d,i) {
                return {
                    x:d.idx,
                    y:d.price
                }
            })

            const model = tf.sequential();
            model.add(tf.layers.dense({inputShape:[1], units:1, useBias:true}));
            model.add(tf.layers.dense({units: 50, activation:'sigmoid'}));
            model.add(tf.layers.dense({units: 1, useBias: true}));
            function convertToTensor(data) {
                // Wrapping these calculations in a tidy will dispose any 
                // intermediate tensors.

                return tf.tidy(() => {
                    // Step 1. Shuffle the data    
                    tf.util.shuffle(data);

                    // Step 2. Convert data to Tensor
                    const inputs = data.map(d => d.idx)
                    const labels = data.map(d => d.price);

                    const inputTensor = tf.tensor2d(inputs, [inputs.length, 1]);
                    const labelTensor = tf.tensor2d(labels, [labels.length, 1]);

                    //Step 3. Normalize the data to the range 0 - 1 using min-max scaling
                    const inputMax = inputTensor.max();
                    const inputMin = inputTensor.min();  
                    const labelMax = labelTensor.max();
                    const labelMin = labelTensor.min();

                    const normalizedInputs = inputTensor.sub(inputMin).div(inputMax.sub(inputMin));
                    const normalizedLabels = labelTensor.sub(labelMin).div(labelMax.sub(labelMin));

                    return {
                        inputs: normalizedInputs,
                        labels: normalizedLabels,
                        // Return the min/max bounds so we can use them later.
                        inputMax,
                        inputMin,
                        labelMax,
                        labelMin,
                    }
                });  
            }

            function trainModel(model, inputs, labels) {
                // Prepare the model for training.  
                model.compile({
                    optimizer: tf.train.adam(),
                    loss: tf.losses.meanSquaredError,
                    metrics: ['mse'],
                });
                
                const batchSize = 32;
                const epochs = 50;
                
                return model.fit(inputs, labels, {
                    batchSize,
                    epochs,
                    shuffle: true
                })
            }

            const tensorData = convertToTensor(data);
            const {inputs, labels} = tensorData;

            function testModel(model, inputData, normalizationData) {
                const {inputMax, inputMin, labelMin, labelMax} = normalizationData;  
                
                // Generate predictions for a uniform range of numbers between 0 and 1;
                // We un-normalize the data by doing the inverse of the min-max scaling 
                // that we did earlier.
                const [xs, preds] = tf.tidy(() => {
                    
                    const xs = tf.linspace(0, 1, 100);      
                    const preds = model.predict(xs.reshape([100, 1]));      
                    
                    const unNormXs = xs
                    .mul(inputMax.sub(inputMin))
                    .add(inputMin);
                    
                    const unNormPreds = preds
                    .mul(labelMax.sub(labelMin))
                    .add(labelMin);
                    
                    // Un-normalize the data
                    return [unNormXs.dataSync(), unNormPreds.dataSync()];
                });
                
                
                const predictedPoints = Array.from(xs).map((val, i) => {
                    return {x: val, y: preds[i]}
                });               
                
                console.log(predictedPoints);
            }

            trainModel(model, inputs, labels).then(function(msg) {
                console.log('Done Training!', msg);
                testModel(model, data, tensorData);
            })
        },
        onPrice() {
            this.data_type = 'price';
            this.refresh();
        },
        onResist() {
            this.data_type = 'resist';
            this.refresh();
        },
        onSupport() {
            this.data_type = 'support';
            this.refresh();
        },
        onChangeDate() {
            this.refresh();
        },
        onSetIchimoku() {
            if(this.selected_item.category) {
                common.chart.setIchimoku();
            }
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
                // common.chart.uninit('chart-space');
                // common.chart.init('chart-space', {signal:me.signal,type:me.data_type});
                var to_date = moment(me.end_date).add(1, 'day').format("YYYY-MM-DD")
                api.getData(me.selected_item.category,to_date).then(function(data) {
                    //me.$refs.echart.options
                    // var supstance = []
                    // if(me.selected_item.supstance) {
                    //     supstance = me.selected_item.supstance.split(',');
                    // }
                    // common.chart.load(data, me.end_date, supstance);
                    me.setOptions(data);
                })
            },400)
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
            var dataMA5 = this.calculateMA(5, stocks.data);
            var dataMA10 = this.calculateMA(10, stocks.data);
            var dataMA20 = this.calculateMA(20, stocks.data);
            var dataMA60 = this.calculateMA(60, stocks.data);
            var option = {
                animation: false,
                color: color_list,
                title: {
                    left: 'center'
                },
                legend: {
                    top: 30,
                    data: ['STOCK', 'MA5', 'MA10', 'MA20', 'MA60']
                },
                tooltip: {
                    trigger: 'axis',
                    position: function (pt) {
                        return [pt[0], '10%'];
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
                        },
                        emphasis: {
                        color: '#140'
                        }
                    },
                    data: stocks.volume
                }, {
                    type: 'candlestick',
                    name: 'STOCK',
                    data: stocks.data,
                    itemStyle: {
                        normal: {
                        color: '#ef232a',
                        color0: '#14b143',
                        borderColor: '#ef232a',
                        borderColor0: '#14b143'
                        },
                        emphasis: {
                        color: 'black',
                        color0: '#444',
                        borderColor: 'black',
                        borderColor0: '#444'
                        }
                    }
                }, {
                    name: 'MA5',
                    type: 'line',
                    data: dataMA5,
                    smooth: true,
                    showSymbol: false,
                    lineStyle: {
                        normal: {
                        width: 1
                        }
                    }
                }, {
                    name: 'MA10',
                    type: 'line',
                    data: dataMA10,
                    smooth: true,
                    showSymbol: false,
                    lineStyle: {
                        normal: {
                        width: 1
                        }
                    }
                }, {
                    name: 'MA20',
                    type: 'line',
                    data: dataMA20,
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
                    data: dataMA60,
                    smooth: true,
                    showSymbol: false,
                    lineStyle: {
                        normal: {
                        width: 1
                        }
                    }
                }]
            };
            me.chart_options = option;
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
        console.log(me.$refs.echart);
        //common.chart.init('chart-space', {signal:this.signal});
    },
    beforeUpdate() {

    },
    updated() {
        
    },
    beforeDestroy() {
        //common.chart.uninit();
    },
    destroyed() {
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