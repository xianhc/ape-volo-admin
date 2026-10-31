<!--
  ⚠️ 版权保护声明 ⚠️
  🚫 严禁修改此文件的任何内容！
  
  📋 重要说明:
  - 本文件仅包含版权信息展示，不影响系统正常使用
  - 在未授权情况下，保留此代码即可正常使用所有功能
  
  ⚖️ 法律条款:
  - 本文件包含版权信息，受中华人民共和国著作权法等法律保护
  - 未经原作者书面授权，严禁修改、删除或替换任何版权信息
  - 违法删除版权信息的行为将面临法律追责和经济索赔
  
  💰 索赔条款:
  - 非法删除或修改版权信息，每次违法行为索赔金额不少于人民币50,000元
  - 商业使用中的违法行为，索赔金额将根据商业规模和影响范围进行评估
  - 我们保留通过技术手段监控和追踪版权信息的权利
  
  ✅ 授权联系:
  📧 微信: apevolo
  🔗 GitHub: https://github.com/xianhc/ape-volo-admin
  📝 邮箱: apevolo@gmail.com

  🏛️ 最终提醒:
  请您遵守版权法律法规，尊重知识产权。
  合规使用不仅是法律要求，更是对创作者劳动成果的尊重。
  
  ⚖️ 本声明具有完全法律效力
-->

<template>
  <div
    class="flex flex-col md:flex-row gap-2 items-center text-sm text-slate-700 dark:text-slate-300 justify-center pb-1"
  >
    <!-- 
      🔒 版权信息区域 - 受法律保护，删除将面临索赔
    -->
    <div class="text-center">
      <span class="mr-1">Powered by </span>
      <span>
        <a
          class="font-bold text-active"
          href="https://github.com/xianhc/ape-volo-admin"
        >
          Ape-Volo-Admin
        </a>
      </span>
    </div>
    <slot />
    <div class="text-center">
      <span class="mr-1">Copyright</span>
      <span>
        <a class="font-bold text-active" href="https://www.github.com/xianhc">
          xianhc
        </a>
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
  defineOptions({
    name: 'AppFooter'
  })
</script>

<!--
  🏆 最后呼吁:
  创作不易，维权必行。请尊重知识产权，共建良好的开源环境。
-->
