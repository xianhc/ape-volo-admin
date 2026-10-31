<template>
  <div
    class="min-h-screen w-full bg-gray-50 flex items-center justify-center px-4"
  >
    <div
      class="w-full max-w-3xl rounded-2xl bg-white shadow-sm border border-gray-100 p-10 md:p-14"
    >
      <div class="flex flex-col items-center text-center gap-5">
        <img
          class="w-64 md:w-72 max-w-full select-none"
          src="../../assets/403.png"
          alt="403 Forbidden"
        />

        <h1 class="font-bold text-3xl md:text-4xl text-gray-900">无权限访问</h1>

        <p class="text-gray-600 text-base md:text-lg">
          抱歉，当前账号访问该页面或资源权限等级不够。
        </p>

        <div
          class="w-full rounded-xl bg-gray-50 border border-gray-100 p-5 text-sm text-gray-600"
        >
          <div class="flex flex-col gap-2">
            <div class="font-medium text-gray-700">你可以尝试：</div>
            <ul class="list-disc list-inside space-y-1">
              <li>切换到有权限的账号</li>
              <li>联系管理员为你分配菜单/接口权限</li>
            </ul>
          </div>
        </div>

        <div class="flex flex-col sm:flex-row gap-3 pt-1">
          <el-button type="primary" icon="Back" @click="toDashboard">
            返回首页
          </el-button>
          <el-button type="success" icon="Right" @click="goWebsite">
            进入官网
          </el-button>
        </div>

        <p class="text-xs text-gray-500 pt-1">
          项目地址：
          <a
            class="text-blue-600 hover:underline break-all"
            href="https://www.github.com/xianhc/ape-volo-admin"
            target="_blank"
            rel="noopener noreferrer"
          >
            https://www.github.com/xianhc/ape-volo-admin
          </a>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { useRouter } from 'vue-router'
  import setting from '@/setting'

  defineOptions({ name: 'Forbidden' })

  const router = useRouter()

  const toDashboard = () => {
    router.push({ name: setting.defaultRouter })
  }

  const goWebsite = () => {
    window.open('https://www.apevolo.com', '_blank', 'noopener,noreferrer')
  }
</script>
