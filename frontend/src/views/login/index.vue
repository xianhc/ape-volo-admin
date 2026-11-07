<script setup lang="ts">
  import { computed } from 'vue'
  import AppFooter from '@/components/AppFooter.vue'
  import AppIcon from '@/components/AppIcon.vue'
  import LoginAppearanceTools from './components/LoginAppearanceTools.vue'
  import LoginCodeScene from './components/LoginCodeScene.vue'
  import LoginForm from './components/LoginForm.vue'
  import { useAppStore } from '@/pinia'
  import setting from '@/setting'

  defineOptions({ name: 'Login' })

  const appStore = useAppStore()
  /** 登录表单布局，跟随界面偏好里的登录页布局设置。 */
  const loginLayout = computed(() => appStore.config.loginLayout)
</script>

<template>
  <div class="login-page" :data-layout="loginLayout">
    <div class="login-atmosphere" aria-hidden="true"><div class="login-glow"></div></div>
    <header class="login-header">
      <a class="login-brand" href="https://www.apevolo.com/" target="_blank" rel="noopener noreferrer">
        <img :src="setting.appLogo" alt="ApeVolo" />
        <span class="brand-name"><span class="brand-product">ApeVolo</span> <span class="brand-admin">Admin</span></span>
      </a>
      <LoginAppearanceTools />
    </header>
    <main class="login-main">
      <section class="login-story" aria-label="代码场景"><div class="editor-stage"><LoginCodeScene /></div></section>
      <section class="login-region" aria-labelledby="login-heading">
        <div class="login-content">
          <LoginForm />
          <footer class="login-footer">
            <AppFooter>
              <nav class="login-links" aria-label="文档">
                <a :href="setting.docsUrl" target="_blank" rel="noopener noreferrer"><AppIcon icon="lucide:book-open" /><span>文档</span></a>
                <a :href="setting.githubUrl" target="_blank" rel="noopener noreferrer"><AppIcon icon="lucide:github" /><span>GitHub</span></a>
              </nav>
            </AppFooter>
          </footer>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
  .login-page {
    --login-canvas: #fff;
    --login-ink: #263751;
    --login-muted: #8b99b0;
    --login-border: color-mix(in srgb, var(--el-color-primary) 8%, #e4e7ec);
    --login-field: #ffffffd9;
    --login-toolbar: color-mix(in srgb, var(--el-color-primary) 6%, #f0f2f5);
    --login-story-base: color-mix(in srgb, var(--el-color-primary) 7%, #f1f3f6);
    --login-story-glow: color-mix(in srgb, var(--el-color-primary) 30%, transparent);
    position: relative;
    isolation: isolate;
    min-height: 100svh;
    overflow: hidden;
    color: var(--login-ink);
    background: radial-gradient(ellipse at 100% 100%, color-mix(in srgb, var(--el-color-primary) 5.5%, transparent), transparent 50%), var(--login-canvas);
    font-family: var(--app-font-sans);
  }
  .login-page :deep(*) { box-sizing: border-box; }
  .login-page a { color: inherit; text-decoration: none; }
  .login-page a:focus-visible { outline: 2px solid var(--el-color-primary); outline-offset: 3px; }
  .login-atmosphere { position: absolute; inset: 0 auto 0 0; width: 60%; z-index: -1; overflow: hidden; background: var(--login-story-base); }
  .login-glow { position: absolute; inset: -12%; background: linear-gradient(154deg, transparent 22%, var(--login-story-glow) 48%, transparent 76%); filter: blur(70px); animation: login-drift 28s ease-in-out infinite alternate; }
  .login-header { position: absolute; inset: 0 0 auto; z-index: 20; display: flex; align-items: center; justify-content: space-between; gap: 12px; height: 74px; padding: 0 18px; pointer-events: none; }
  .login-header > * { pointer-events: auto; }
  .login-brand { display: flex; align-items: center; gap: 9px; min-width: 0; }
  .login-brand img { width: 39px; height: 38px; flex-shrink: 0; object-fit: contain; border-radius: 8px; mix-blend-mode: multiply; }
  .brand-name { font: 400 21px/1.2 'Segoe UI', sans-serif; letter-spacing: -.35px; white-space: nowrap; }
  .brand-product { font-family: inherit; font-size: inherit; font-weight: 650; }
  .brand-admin { margin-left: 2px; font-family: inherit; font-size: inherit; font-weight: 400; }
  .login-main { display: grid; grid-template-columns: 60% 40%; align-items: stretch; min-height: 100svh; }
  .login-story { display: flex; align-items: center; justify-content: center; min-width: 0; padding: 82px 30px 88px; }
  .editor-stage { width: 640px; max-width: 100%; }
  .login-region { display: flex; align-items: center; justify-content: center; min-width: 0; padding: 95px 32px 88px; background: var(--login-canvas); }
  .login-content { width: 100%; max-width: 448px; }
  .login-footer { margin-top: 30px; color: #53617a; text-align: center; }
  .login-footer :deep(> div) { flex-direction: column; gap: 15px; padding: 0; }
  .login-footer :deep(.text-center) { color: #64728b; font: 400 12px/1.5 'Segoe UI', sans-serif; }
  .login-footer :deep(.text-center:first-child) { color: #53617a; font-size: 13px; }
  .login-footer :deep(.text-center:first-child a) { color: var(--el-color-primary); text-transform: uppercase; font-weight: 500; }
  .login-footer :deep(.text-center:last-child a) { color: #53617a; font-weight: 600; }
  .login-footer :deep(a:hover) { text-decoration: underline; text-underline-offset: 4px; }
  .login-links { display: flex; align-items: center; justify-content: center; gap: 26px; color: #72809a; font-size: 11px; }
  .login-links a { display: flex; align-items: center; gap: 6px; }
  .login-links :deep(svg) { width: 17px; height: 17px; }
  .login-links a:hover { color: var(--el-color-primary); }
  .login-page[data-layout=left] .login-main { grid-template-columns: 40% 60%; }
  .login-page[data-layout=left] .login-region { grid-column: 1; grid-row: 1; }
  .login-page[data-layout=left] .login-story { grid-column: 2; grid-row: 1; }
  .login-page[data-layout=left] .login-atmosphere { left: auto; right: 0; }
  .login-page[data-layout=center] .login-atmosphere, .login-page[data-layout=center] .login-story { display: none; }
  .login-page[data-layout=center] .login-main { display: flex; align-items: center; justify-content: center; padding: 108px 24px 76px; }
  .login-page[data-layout=center] .login-region { width: 100%; max-width: 520px; padding: 0; background: transparent; }
  .login-page[data-layout=center] .login-content { max-width: none; padding: 33px 38px 30px; border: 1px solid var(--login-border); border-radius: 24px; background: var(--login-canvas); box-shadow: 0 18px 65px color-mix(in srgb, var(--el-color-primary) 7%, transparent); }
  :global(html.dark .login-page) {
    --login-canvas: color-mix(in srgb, var(--el-color-primary) 4%, #14171f);
    --login-ink: #dce5f7;
    --login-muted: #8b9bb5;
    --login-border: color-mix(in srgb, var(--el-color-primary) 14%, #303640);
    --login-field: color-mix(in srgb, var(--el-color-primary) 7%, #1a202c);
    --login-toolbar: color-mix(in srgb, var(--el-color-primary) 12%, #252c38);
    --login-story-base: color-mix(in srgb, var(--el-color-primary) 3%, #070709);
    --login-story-glow: color-mix(in srgb, var(--el-color-primary) 20%, transparent);
  }
  :global(html.dark .login-brand img) { mix-blend-mode: normal; padding: 3px; background: #fff; }
  :global(html.dark .login-links) { color: #94a3b8; }
  @keyframes login-drift { to { transform: translate(12%, -4%) scale(1.12); } }
  @media (min-width: 1600px) { .editor-stage { width: 720px; } }
  @media (min-width: 768px) and (max-width: 1199px) {
    .login-main, .login-page[data-layout=left] .login-main { grid-template-columns: 50% 50%; }
    .login-atmosphere { width: 50%; }
    .login-story { padding-inline: 20px; }
    .login-region { padding-inline: 30px; }
  }
  @media (min-width: 768px) and (max-height: 790px) {
    .login-region { padding-top: 87px; padding-bottom: 70px; }
    .login-footer { margin-top: 25px; }
    .login-footer :deep(> div) { gap: 13px; }
    .editor-stage { width: 570px; }
    .login-page[data-layout=center] .login-main { padding-top: 92px; }
    .login-page[data-layout=center] .login-content { padding-top: 25px; padding-bottom: 23px; }
  }
  @media (max-width: 767px) {
    .login-header { height: 85px; padding-inline: 21px; }
    .login-brand { gap: 5px; }
    .login-brand img { width: 31px; height: 30px; }
    .brand-name { font-size: 16px; }
    .login-atmosphere, .login-story { display: none !important; }
    .login-main, .login-page[data-layout=left] .login-main, .login-page[data-layout=center] .login-main { display: flex; align-items: center; justify-content: center; padding: 112px 27px 70px; }
    .login-region, .login-page[data-layout=center] .login-region { width: 100%; max-width: 400px; padding: 0; background: transparent; }
    .login-content, .login-page[data-layout=center] .login-content { max-width: none; padding: 0; border: 0; border-radius: 0; box-shadow: none; background: transparent; }
    .login-footer { margin-top: 28px; }
  }
  @media (max-width: 360px) {
    .login-header { padding-inline: 14px; gap: 8px; }
    .brand-name { font-size: 15px; }
  }
  @media (max-width: 767px) and (max-height: 740px) {
    .login-header { height: 76px; }
    .login-main, .login-page[data-layout=left] .login-main, .login-page[data-layout=center] .login-main { padding-top: 91px; padding-bottom: 54px; }
    .login-footer { margin-top: 24px; }
    .login-footer :deep(> div) { gap: 13px; }
  }
  @media (prefers-reduced-motion: reduce) { .login-glow { animation: none; } }
</style>
