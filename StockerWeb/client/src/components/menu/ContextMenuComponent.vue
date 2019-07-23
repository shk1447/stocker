<template>
<div class="context-menu-wrapper" v-if="activeContextMenu" v-bind:style="{top:top,left:left}">
  <ul class="menu-options">
    <li class="menu-option" v-for="item in menu_items" v-on:click="handleClickMenu(item)">{{item.label}}</li>
  </ul>
</div>
</template>

<script>
import api from '../../api/api.js'
import _ from 'lodash';
import moment from 'moment';
export default {
    props: {
        actionMenu: { type : Function }
    },
    data () {
        return {
            menu_items : [
                            {id:'auto',label:'Auto'},
                            {id:'manual',label:'Manual'},
                            {id:'reset',label:'Reset'},
                            {id:'zoom_reset',label:'Reset Zoom'}
                        ],
            activeContextMenu:false,
            top:'0px',
            left:'0px',
            params : {},
            analysisDate:new Date()
        }
    },
    components:{
        
    },
    methods: {
        handleClickMenu(item) {
            var me = this;
            switch(item.id) {
                case 'reset' : 
                    common.view.clear();
                break;
                case 'zoom_reset' :
                    common.view.zoom_reset();
                break;
                case 'search' :
                    common.events.emit('message', {type:'warning' , message:'Not implemented.'})
                break;
                case 'auto' :
                    console.log(this.params.node_info);
                    if(this.params.node_info && this.params.node_info.type === "date") {
                        var analysis_date = moment(this.params.node_info.id).add(1, 'day').format("YYYY-MM-DD");
                        var nodes = common.view.getNodes();
                        var alarm_items = {};
                        _.each(nodes, function(d,i) {
                            if(!d.type) {
                                count++;
                                api.getData(d.id, analysis_date).then(function(data) {
                                    var ad = common.chart.analysis(data, moment(me.params.node_info.id).format("YYYY-MM-DD"), d.supstance);
                                    if(parseFloat(ad.prev_buy.props.last_resist) < ad.current.Low) {
                                        //console.log(d.name, ad);
                                        if(parseFloat(ad.prev_buy.props.last_support) < ad.current.Close) {
                                            console.log(d.name, ' ', ad.prev_buy.props.last_support, '원에서 지지받았으면, ',
                                            ad.current.props.last_resist, ' ~ ', ad.current.props.last_support, '원에서 매수해라!');
                                        }
                                    }
                                })
                            }
                        })
                    } else {
                        common.events.emit('message', {type:'warning' , message:'PLEASE SELECT DATE NODE!'})
                    }
                break;
                case 'manual' :
                    var nodes = common.view.getNodes();
                    var links = common.view.getLinks();
                    var count = 0;
                    var alarm_items = {};
                    me.$loading({})
                    try {
                        var test = {};
                        _.each(links, function(d,i) {
                            if(test[d.target]) {
                                test[d.target]++;
                            } else {
                                test[d.target] = 1;
                            }
                        });
                        _.each(test, function(d,i) {
                            if(d > 1) {
                                alarm_items[i] = d;
                            }
                        });
                        common.view.setAlarm(alarm_items);
                        me.$loading({}).close();
                    } catch (error) {
                        me.$loading({}).close();
                    }
                break;
            }
            me.activeContextMenu = false;
        },
        handleContextMenu(d) {
            var me = this;
            me.left = d.left + 'px';
            me.top = d.top + 'px';
            me.params = d.params;
            me.activeContextMenu = d.active;
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
        common.events.on('contextmenu', me.handleContextMenu)
        common.events.on('changeDate', function(d) {
            me.analysisDate = d.date;
        })
    },
    beforeUpdate() {

    },
    updated() {

    },
    beforeDestroy() {

    },
    destroyed() {
        var me = this;
        console.log('destroyed')
        common.events.off('contextmenu', me.handleContextMenu);
    }
}
</script>
<style scoped>

.context-menu-wrapper {
    position: absolute;
    z-index: 1000;
}

.menu-options {
    background: #FAFAFA;
    border: 1px solid #BDBDBD;
    box-shadow: 0 2px 2px 0 rgba(0,0,0,.14),0 3px 1px -2px rgba(0,0,0,.2),0 1px 5px 0 rgba(0,0,0,.12);
    display: block;
    list-style: none;
    margin: 0;
    padding: 0;
    position: absolute;
    width: 150px;
    z-index: 999999;
}

.menu-options li {
    border-bottom: 1px solid #E0E0E0;
    margin: 0;
    padding: 5px 35px;
    cursor: pointer;
}

.menu-options li:last-child {
    border-bottom: none;
}

.menu-options li:hover {
    background: #1E88E5;
    color: #FAFAFA;
}

</style>
