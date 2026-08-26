function echatrsPie(title, xdata,series) {
    var options = {
        title:title,
            //{
            //text: '某站点用户访问来源',
            //subtext: '仅供测试',
            //x: 'center'
            //},
        textStyle: {
            //color: '',
            //fontFamily: '',
            fontSize: '50',
            fontWeight: 'bolder'
        },
        tooltip: {
            trigger: 'item',
            formatter: "{a} <br/>{b} : {c}元 ({d}%)"
        },
        legend: {
            orient: 'vertical',
            x: 'left',
            data: xdata//['直接访问', '邮件营销', '联盟广告', '视频广告', '搜索引擎']
        },
        toolbox: {
            show: true,
            feature: {
                mark: { show: true },
                dataView: { show: true, readOnly: false },
                magicType: {
                    show: true,
                    type: ['pie', 'funnel'],
                    option: {
                        funnel: {
                            x: '25%',
                            width: '50%',
                            funnelAlign: 'left',
                            max: 1548
                        }
                    }
                },
                restore: { show: true },
                saveAsImage: { show: true }
            }
        },
        calculable: true,
        series:series
        //    [
        //    {
        //        name: '访问来源',
        //        type: 'pie',
        //        radius: '55%',
        //        center: ['50%', '60%'],
        //        data:[
        //            { value: 335, name: '直接访问' },
        //            { value: 310, name: '邮件营销' },
        //            { value: 234, name: '联盟广告' },
        //            { value: 135, name: '视频广告' },
        //            { value: 1548, name: '搜索引擎' }
        //        ]
        //    }
        //]
    };
    return options;
}

function chartsBar(xdata, ydata) {
    var options = {
        title: {
            text: '近一年消费统计',
            subtext: ''
        },
        tooltip: {
            trigger: 'axis',
            formatter: "{a} <br/>{b} : {c}元"
        },
        legend: {
            data: ['消费量']
        },
        toolbox: {
            show: true,
            feature: {
                mark: { show: true },
                dataView: { show: true, readOnly: false },
                magicType: { show: true, type: ['line', 'bar'] },
                restore: { show: true },
                saveAsImage: { show: true }
            }
        },
        calculable: true,
        xAxis: [
            {
                axisLabel: {
                    rotate: 40
                },
                type: 'category',
                data: xdata
            }
        ],
        yAxis: [
            {
                type: 'value'
            }
        ],
        series: [
            {
                name: '消费量',
                type: 'bar',
                data: ydata,
                markPoint: {
                    data: [
                        { type: 'max', name: '最大值' },
                        { type: 'min', name: '最小值' }
                    ]
                },
                markLine: {
                    data: [
                        { type: 'average', name: '平均值' }
                    ]
                }
            }
        ]
    };

    return options;
}