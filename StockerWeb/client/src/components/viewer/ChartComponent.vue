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
    </div>
</div>
</template>

<script>

import * as tf from '@tensorflow/tfjs';
import moment from 'moment';
import api from '../../api/api.js';
import { setTimeout } from 'timers';
// import { func } from '@tensorflow/tfjs-data';
// import { constants } from 'fs';

export default {
    data () {
        return {
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
                common.chart.uninit('chart-space');
                common.chart.init('chart-space', {signal:me.signal,type:me.data_type});
                var to_date = moment(me.end_date).add(1, 'day').format("YYYY-MM-DD")
                api.getData(me.selected_item.category,to_date).then(function(data) {
                    var supstance = []
                    if(me.selected_item.supstance) {
                        supstance = me.selected_item.supstance.split(',');
                    }
                    common.chart.load(data, me.end_date, supstance);
                })
            },400)
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
        common.chart.init('chart-space', {signal:this.signal});
    },
    beforeUpdate() {

    },
    updated() {
        
    },
    beforeDestroy() {
        common.chart.uninit();
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

</style>