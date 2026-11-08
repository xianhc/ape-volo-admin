<!--
******************************************************************************
 * 项目名称：APE-VOLO-ADMIN
 * 开源地址：https://github.com/xianhc/ape-volo-admin
 * 系统官网：https://www.apevolo.com
 * 联系邮箱：apevolo@gmail.com
 *
 * Copyright © 2022 xianhc（冼浩春）。保留所有权利。
 * SPDX-License-Identifier: Apache-2.0
 *
 * 本项目采用 Apache License 2.0，完整许可条款详见根目录 LICENSE 文件。
 *
 * 版权与署名保留要求（Apache License 2.0 第 4 条）：
 * 再分发本项目或其衍生作品时，须向接收者提供许可证副本。
 * 以源码形式分发衍生作品时，须保留与该作品相关的原始版权及署名通知，
 * 不得删除原始通知或以他人署名替换；修改过的文件须显著注明已作修改。
 * 上述说明为许可证要求的提示，完整义务以 LICENSE 条款为准。
 *
 * 作者保留依法维护自身合法权益的权利。授权与合作事宜请通过上述邮箱联系。
 ******************************************************************************
-->

<template>
  <div
    class="app-footer flex flex-col md:flex-row gap-2 items-center justify-center pb-1"
  >
    <div class="text-center">
      <span class="mr-1">Powered by </span>
      <span>
        <a
          class="app-footer-link"
          href="https://github.com/xianhc/ape-volo-admin"
        >
          Ape-Volo-Admin
        </a>
      </span>
    </div>
    <slot />
    <div class="text-center">
      <span class="mr-1">Copyright © 2022–{{ currentYear }}</span>
      <span>
        <a class="app-footer-link" href="https://www.github.com/xianhc">
          xianhc
        </a>
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
  const currentYear = new Date().getFullYear()

  defineOptions({
    name: 'AppFooter'
  })
</script>

<style scoped>
  .app-footer {
    color: var(--app-text-secondary);
    font: 400 13px/1.5 'Segoe UI', sans-serif;
  }

  .app-footer > .text-center {
    font: inherit;
  }

  .app-footer-link {
    color: var(--el-color-primary);
    font-weight: 600;
  }
</style>
