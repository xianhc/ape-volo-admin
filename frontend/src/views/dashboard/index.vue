<template>
  <div class="p-6">
    <el-card shadow="never" class="mt-2 ape-volo-content">
      <el-row class="h-30">
        <el-col :span="18" :xs="24">
          <div class="flex items-start justify-start">
            <img
              class="w-20 h-20 rounded-full"
              :src="getStaticBaseUrl() + userStore.userInfo.headerImg"
            />
            <div class="ml-5 mt-4">
              <p class="text-xl font-bold">{{ greetings }}</p>
              <p class="text-sm mt-2 text-gray">今日晴，20℃ - 32℃！</p>
            </div>
          </div>
        </el-col>

        <el-col :span="6" :xs="24">
          <div
            class="flex flex-row items-center justify-end h-full pr-3 space-x-8"
          >
            <div class="flex flex-col items-center">
              <span class="text-2xl font-bold text-blue-600">2/10</span>
              <span class="text-xs text-gray-500">待办</span>
            </div>
            <div class="flex flex-col items-center">
              <span class="text-2xl font-bold text-green-600">8</span>
              <span class="text-xs text-gray-500">项目</span>
            </div>
            <div class="flex flex-col items-center">
              <span class="text-2xl font-bold text-orange-600">300</span>
              <span class="text-xs text-gray-500">团队</span>
            </div>
          </div>
        </el-col>
      </el-row>
    </el-card>

    <!-- 快捷导航 -->
    <el-row :gutter="10" class="mt-5">
      <el-col :span="8">
        <el-card shadow="never" class="ape-volo-content">
          <template #header>
            <span class="text-gray">快捷导航</span>
          </template>
          <div
            class="dark:text-neutral-400 rounded shadow p-4 border border-gray-200 dark:border-neutral-700"
          >
            <div
              class="grid grid-cols-2 md:grid-cols-3 3xl:grid-cols-4 gap-8 border"
            >
              <div
                v-for="(item, index) in menuNavigations"
                :key="index"
                class="flex flex-col p-6 min-h-[110px] items-center group cursor-pointer"
                @click="toPath(item)"
              >
                <div
                  class="w-12 h-12 rounded border-2 border-gray-300 dark:border-neutral-500 bg-gray-200 dark:bg-gray-200 flex items-center justify-center group-hover:bg-blue-400 group-hover:text-white group-hover:border-blue-400 transition-all duration-200"
                >
                  <el-icon class="text-2xl">
                    <AppIcon :icon="item.icon" />
                  </el-icon>
                </div>
                <div
                  class="text-sm mt-3 text-gray-700 dark:text-gray-300 font-medium"
                >
                  {{ item.title }}
                </div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="16">
        <el-card shadow="never" class="ape-volo-content">
          <template #header>
            <span class="text-gray">项目</span>
          </template>
          <div>
            <div
              class="grid grid-rows-2 grid-cols-3 divide-x divide-y cursor-pointer dark:text-neutral-400 rounded shadow border border-gray-200 dark:border-neutral-700"
            >
              <div
                v-for="(item, index) in projectItems"
                :key="index"
                class="flex flex-col justify-between p-6 min-h-[140px]"
              >
                <div class="flex items-center mb-2">
                  <el-icon class="text-3xl mr-2">
                    <AppIcon :icon="item.icon" />
                  </el-icon>
                  <span class="text-xl font-bold">{{ item.title }}</span>
                </div>
                <div class="flex-1 text-base mb-2">
                  {{ item.desc }}
                </div>
                <div class="flex justify-between items-center text-sm mt-2">
                  <span>{{ item.tag }}</span>
                  <span>{{ item.date }}</span>
                </div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="10" class="mt-5">
      <!-- 访问趋势统计图 -->
      <el-col :xs="24" :span="16">
        <el-card class="ape-volo-content">
          <template #header>
            <div class="flex items-center justify-between">
              <span>访问趋势</span>
              <el-radio-group v-model="visitTrendDateRange">
                <el-radio-button label="近7天" :value="7" />
                <el-radio-button label="近30天" :value="30" />
              </el-radio-group>
            </div>
          </template>
          <div class="h-[400px]">
            <ECharts :options="visitTrendChartOptions" height="400px" />
          </div>
        </el-card>
      </el-col>
      <!-- 更新日志 -->
      <el-col :xs="24" :span="8">
        <el-card class="ape-volo-content">
          <template #header>
            <div class="flex items-center justify-between">
              <span>更新日志</span>
              <el-link
                type="primary"
                :underline="false"
                href="https://www.apevolo.com/change-log.html"
                target="_blank"
                rel="noopener noreferrer"
              >
                完整记录
                <el-icon><AppIcon icon="ep:top-right" /></el-icon>
              </el-link>
            </div>
          </template>

          <el-scrollbar height="410px">
            <el-timeline class="p-3">
              <el-timeline-item
                v-for="(item, index) in changeLogList"
                :key="index"
                :timestamp="item.date"
                placement="top"
                :color="index === 0 ? '#67C23A' : '#909399'"
                :hollow="index !== 0"
              >
                <div
                  class="changeLog-item"
                  :class="{ 'latest-item': index === 0 }"
                >
                  <div>
                    <el-text tag="strong">{{ item.version }}</el-text>
                    <el-tag
                      v-if="item.tag"
                      :type="index === 0 ? 'success' : 'info'"
                      size="small"
                    >
                      {{ item.tag }}
                    </el-tag>
                  </div>

                  <el-text class="changeLog-content">
                    {{ item.content }}
                  </el-text>

                  <div v-if="item.link">
                    <el-link
                      :type="index === 0 ? 'primary' : 'info'"
                      :href="item.link"
                      target="_blank"
                      rel="noopener noreferrer"
                      :underline="false"
                    >
                      详情
                      <el-icon class="link-icon"><AppIcon icon="ep:top-right" /></el-icon>
                    </el-link>
                  </div>
                </div>
              </el-timeline-item>
            </el-timeline>
          </el-scrollbar>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
  defineOptions({
    name: 'Dashboard'
  })

  import { ref, watch, computed, defineAsyncComponent } from 'vue'
  import { getVisitTrend } from '@/api/log/operateLog'
  import type { VisitTrendData } from '@/api/log/types/operateLog.types'
  import { useUserStore } from '@/pinia/modules/user'
  import { getStaticBaseUrl } from '@/utils/index'
  import { useRouter } from 'vue-router'
  const ECharts = defineAsyncComponent(() => import('@/components/Echarts.vue'))
  const router = useRouter()

  const userStore = useUserStore()

  // 更新记录
  const changeLogList = ref([
    {
      version: 'v3.5.2',
      date: '2026-04-12',
      content: '前端重构为TS版本，UnoCSS替换Tailwind CSS，用户偏好设置持久化。',
      link: 'https://www.apevolo.com/change-log.html',
      tag: '里程碑'
    },
    {
      version: 'v3.5.1',
      date: '2026-02-06',
      content:
        '前端重新设计与实现，全面升级至 Vue 3 + Vite 构建体系，大幅提升开发体验和运行性能。',
      link: 'https://www.apevolo.com/change-log.html',
      tag: '里程碑'
    },
    {
      version: 'v3.4.2',
      date: '2025-05-06',
      content: '优化日库库与多租户库操作，API接口文档增加响应模型。',
      link: 'https://www.apevolo.com/change-log.html',
      tag: '里程碑'
    },
    {
      version: 'v3.4.1',
      date: '2025-05-03',
      content: '移除自动更新工作流，优化项目结构。',
      link: 'https://www.apevolo.com/change-log.html',
      tag: '里程碑'
    },
    {
      version: 'v3.3.0',
      date: '2025-04-29',
      content: '后端增加本地化语言支持。',
      link: 'https://www.apevolo.com/change-log.html',
      tag: '里程碑'
    }
  ])

  // 当前时间
  const currentDate = new Date()
  const greetings = computed(() => {
    const hours = currentDate.getHours()
    const nickname = userStore.userInfo.nickName
    if (hours >= 6 && hours < 8) {
      return `早安，${nickname}，新的一天开始啦！🌅`
    } else if (hours >= 8 && hours < 12) {
      return `上午好，${nickname}，愿你今天活力满满！`
    } else if (hours >= 12 && hours < 14) {
      return `中午好，${nickname}，记得午休哦～`
    } else if (hours >= 14 && hours < 18) {
      return `下午好，${nickname}，继续加油！`
    } else if (hours >= 18 && hours < 22) {
      return `晚上好，${nickname}，辛苦啦，注意休息！`
    } else if (hours >= 22 && hours < 24) {
      return `夜深了，${nickname}，早点休息，晚安🌙`
    } else {
      return `夜已深，${nickname}，愿你有个好梦，晚安🌛！`
    }
  })

  // 访问趋势日期范围（单位：天）
  const visitTrendDateRange = ref(7)
  // 访问趋势图表配置
  const visitTrendChartOptions = ref({})

  /**
   * 获取访问趋势数据，并更新图表配置
   */
  const fetchVisitTrendData = () => {
    getVisitTrend(visitTrendDateRange.value).then((res) => {
      updateVisitTrendChartOptions(res.data)
    })
  }

  /**
   * 更新访问趋势图表的配置项
   *
   * @param data - 访问趋势数据
   */
  const updateVisitTrendChartOptions = (data: VisitTrendData) => {
    visitTrendChartOptions.value = {
      tooltip: {
        trigger: 'axis'
      },
      legend: {
        data: ['行为记录', '异常记录'],
        bottom: 0
      },
      grid: {
        left: '1%',
        right: '5%',
        bottom: '10%',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        data: data.dateList
      },
      yAxis: {
        type: 'value',
        splitLine: {
          show: true,
          lineStyle: {
            type: 'dashed'
          }
        }
      },
      series: [
        {
          name: '行为记录',
          type: 'line',
          data: data.operateList,
          areaStyle: {
            color: 'rgba(64, 158, 255, 0.1)'
          },
          smooth: true,
          itemStyle: {
            color: '#4080FF'
          },
          lineStyle: {
            color: '#4080FF'
          }
        },
        {
          name: '异常记录',
          type: 'line',
          data: data.exceptionList,
          areaStyle: {
            color: 'rgba(103, 194, 58, 0.1)'
          },
          smooth: true,
          itemStyle: {
            color: '#FF0000'
          },
          lineStyle: {
            color: '#FF0000'
          }
        }
      ]
    }
  }

  // 监听访问趋势日期范围的变化，重新获取趋势数据
  watch(
    () => visitTrendDateRange.value,
    () => {
      fetchVisitTrendData()
    },
    { immediate: true }
  )

  const toPath = (item: any) => {
    router.push({ name: item.path })
  }

  const menuNavigations = [
    {
      icon: 'user',
      title: '用户管理',
      path: 'user'
    },
    {
      icon: 'menu',
      title: '菜单管理',
      path: 'menu'
    },
    {
      icon: 'link',
      title: 'API管理',
      path: 'api'
    },
    {
      icon: 'role',
      title: '角色管理',
      path: 'role'
    },
    {
      icon: 'job',
      title: '岗位管理',
      path: 'job'
    },
    {
      icon: 'timer',
      title: '作业调度',
      path: 'timing'
    }
  ]

  const projectItems = [
    {
      icon: 'github',
      title: 'Github',
      desc: '不要等待机会，而要创造机会。',
      tag: '开源组',
      date: '2025-01-01'
    },
    {
      icon: 'vue1',
      title: 'Vue',
      desc: '现在的你决定将来的你。',
      tag: '算法组',
      date: '2025-01-01'
    },
    {
      icon: 'html5',
      title: 'Html5',
      desc: '没有什么比努力更重要。',
      tag: '上班摸鱼',
      date: '2025-01-01'
    },
    {
      icon: 'angular',
      title: 'Angular',
      desc: '热情和欲望可以突破一切难关。',
      tag: 'UI',
      date: '2025-01-01'
    },
    {
      icon: 'react',
      title: 'React',
      desc: '健康的身体是实现目标的基石。',
      tag: '技术牛',
      date: '2025-01-01'
    },
    {
      icon: 'js',
      title: 'Js',
      desc: '路是走出来的，而不是空想出来的。',
      tag: '架构组',
      date: '2025-01-01'
    }
  ]
</script>

<style lang="scss" scoped>
  .changeLog-item {
    padding: 16px;
    margin-bottom: 12px;
    background: var(--el-fill-color-lighter);
    border-radius: 8px;
    transition: all 0.2s;

    &.latest-item {
      background: var(--el-color-primary-light-9);
      border: 1px solid var(--el-color-primary-light-5);
    }
    &:hover {
      transform: translateX(5px);
    }
    .changeLog-content {
      margin-bottom: 12px;
      font-size: 13px;
      line-height: 1.5;
      color: var(--el-text-color-secondary);
    }
  }
</style>
