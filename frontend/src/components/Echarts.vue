<!--
  ECharts 图表组件
  基于 Apache ECharts 的 Vue 3 封装组件，提供响应式图表显示功能
  
  功能特性：
  - 支持柱状图、折线图、饼图等多种图表类型
  - 自动响应容器尺寸变化
  - 支持图表配置的动态更新
  - 自动管理图表实例的创建和销毁
  - 性能优化的渲染机制
  
  使用场景：
  - 数据可视化展示
  - 报表图表显示
  - 仪表盘组件
  - 统计分析界面
-->
<template>
  <!-- 图表容器，支持自定义宽度和高度 -->
  <div ref="chartRef" :style="{ width, height }"></div>
</template>

<script setup lang="ts">
  import { ref, onMounted, nextTick, watch, onBeforeUnmount } from 'vue'

  // ECharts 核心模块导入
  import * as echarts from 'echarts/core'
  import type { EChartsType } from 'echarts/core'

  // 按需导入图表类型，减少打包体积
  import { BarChart, LineChart, PieChart } from 'echarts/charts'

  // 按需导入组件
  import {
    GridComponent, // 网格组件
    TooltipComponent, // 提示框组件
    LegendComponent // 图例组件
  } from 'echarts/components'

  // 渲染器
  import { CanvasRenderer } from 'echarts/renderers'

  // 兼容层：ECharts 6 把 grid.containLabel 移出核心，需显式注册才能沿用旧行为
  import { LegacyGridContainLabel } from 'echarts/features'

  // 响应式尺寸监听
  import { useResizeObserver } from '@vueuse/core'

  // 注册 ECharts 所需的组件和渲染器
  echarts.use([
    CanvasRenderer,
    BarChart,
    LineChart,
    PieChart,
    GridComponent,
    TooltipComponent,
    LegendComponent,
    LegacyGridContainLabel
  ])

  /**
   * 组件属性定义
   */
  const props = defineProps({
    /**
     * ECharts 配置选项
     * @type {Object}
     * @required
     * @description 包含图表类型、数据、样式等完整配置的对象
     * @example
     * {
     *   title: { text: '示例图表' },
     *   xAxis: { data: ['Mon', 'Tue', 'Wed'] },
     *   yAxis: {},
     *   series: [{ data: [120, 200, 150], type: 'bar' }]
     * }
     */
    options: {
      type: Object,
      required: true
    },
    /**
     * 图表容器宽度
     * @type {String}
     * @default '100%'
     * @description 支持像素值（如 '500px'）或百分比（如 '100%'）
     */
    width: {
      type: String,
      default: '100%'
    },
    /**
     * 图表容器高度
     * @type {String}
     * @default '300px'
     * @description 支持像素值（如 '400px'）或百分比（如 '100%'）
     */
    height: {
      type: String,
      default: '300px'
    }
  })

  // DOM 元素引用
  const chartRef = ref(null)

  // ECharts 实例，用于图表的操作和管理
  let chartInstance: EChartsType | null = null

  /**
   * 初始化图表实例
   * @function initChart
   * @description 创建 ECharts 实例并设置初始配置
   * 只在 DOM 元素存在且配置有效时执行
   */
  const initChart = () => {
    if (chartRef.value) {
      // 创建图表实例
      chartInstance = echarts.init(chartRef.value)

      // 设置图表配置
      if (props.options) {
        chartInstance.setOption(props.options)
      }
    }
  }

  /**
   * 监听容器尺寸变化，自动调整图表大小
   * 使用 VueUse 的 useResizeObserver 实现高性能的尺寸监听
   */
  useResizeObserver(chartRef, () => {
    chartInstance?.resize()
  })

  /**
   * 监听配置选项变化，动态更新图表
   * 使用深度监听确保嵌套属性的变化也能被捕获
   */
  watch(
    () => props.options,
    (newOptions) => {
      if (chartInstance && newOptions) {
        chartInstance.setOption(newOptions)
      }
    },
    { deep: true }
  )

  /**
   * 组件挂载后初始化图表
   * 使用 nextTick 确保 DOM 完全渲染后再创建图表实例
   */
  onMounted(() => {
    nextTick(() => initChart())
  })

  /**
   * 组件卸载前清理图表实例
   * 避免内存泄漏，释放 ECharts 实例占用的资源
   */
  onBeforeUnmount(() => {
    chartInstance?.dispose()
  })
</script>
