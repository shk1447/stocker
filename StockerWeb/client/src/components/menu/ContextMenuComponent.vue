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
                            {id:'auto',label:'Analysis'},
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
                    var nodes = common.view.getNodes();
                    var count = 0;
                    var alarm_items = {};
                    function isRange(x, min, max) {
                        return ((x-min) * (x-max) <= 0);
                    }
                    _.each(nodes, function(d,i) {
                        if(!d.type) {
                            count++;
                            api.getData(d.id, moment().add(1, 'day').format("YYYY-MM-DD")).then(function(data) {
                                var analysis_data = common.chart.analysis(data, moment(me.analysisDate).format("YYYY-MM-DD"), d.supstance);

                                if(analysis_data.signal) {
                                    alarm_items[d.id] = 1.2;
                                }

                                count--;
                                if(count === 0) {
                                    common.view.setAlarm(alarm_items);
                                }
                            })
                        }
                    })
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
