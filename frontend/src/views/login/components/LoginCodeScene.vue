<script setup lang="ts">
import AppIcon from '@/components/AppIcon.vue'
/** 编码插画展示的产品标识与版本，均为展示文案，不参与登录。 */
interface CodeSceneProps {
  /** 产品的代码标识。 */
  productName?: string
  /** 编码插画中的版本文案。 */
  version?: string
}

withDefaults(defineProps<CodeSceneProps>(), {
  productName: 'ape-volo-admin',
  version: 'xianhc',
})
</script>

<template>
  <figure class="code-scene" role="img" :aria-label="'代码场景'">
    <div class="scene-composition" aria-hidden="true">
      <div class="code-window">
        <div class="window-bar">
          <span class="window-controls"><i></i><i></i><i></i></span>
          <span class="file-name"><span class="language-badge">C#</span>Program.cs</span>
          <AppIcon class="file-mark" icon="lucide:code" />
        </div>
        <div class="code-body">
          <div class="code-line"><span class="line-number">1</span><span class="source"><span class="syntax-keyword">var</span> app = <span class="syntax-type">WebApplication</span></span></div>
          <div class="code-line"><span class="line-number">2</span><span class="source source-indent">.<span class="syntax-method">CreateBuilder</span>(args).<span class="syntax-method">Build</span>();</span></div>
          <div class="code-line code-section"><span class="line-number">3</span><span class="source">app.<span class="syntax-method">MapGet</span>(<span class="syntax-string">"/"</span>, () =&gt; <span class="syntax-keyword">new</span> {</span></div>
          <div class="code-line"><span class="line-number">4</span><span class="source source-indent">Name = <span class="syntax-string">"{{ productName }}"</span>,</span></div>
          <div class="code-line"><span class="line-number">5</span><span class="source source-indent">Version = <span class="syntax-string">"{{ version }}"</span></span></div>
          <div class="code-line"><span class="line-number">6</span><span class="source syntax-punctuation">});</span></div>
          <div class="code-line"><span class="line-number">7</span><span class="source">app.<span class="syntax-method">Run</span>();<span class="editor-caret"></span></span></div>
        </div>
        <div class="window-status"><span class="status-indicator"></span><span>Ready to build</span><span class="file-encoding">.NET 10 / Vue 3</span></div>
      </div>
    </div>
  </figure>
</template>

<style scoped>
.code-scene {
  --editor-surface: color-mix(in srgb, var(--el-color-primary) 30%, #1c2433);
  --editor-edge: color-mix(in srgb, var(--el-color-primary) 35%, #536173);
  --editor-text: #edf2f7;
  --editor-blue: color-mix(in srgb, var(--el-color-primary) 40%, #fff);
  --editor-mint: #b6ead0;
  --editor-method: #f1dcaa;
  width: 100%;
  margin: 0;
  container-type: inline-size;
}
.scene-composition { position: relative; width: 100%; aspect-ratio: 640 / 510; }
.code-window {
  position: absolute;
  top: 17%;
  left: 7%;
  width: 86%;
  overflow: hidden;
  border: 1px solid var(--editor-edge);
  border-radius: 12px;
  color: var(--editor-text);
  background: var(--editor-surface);
  box-shadow: 0 22px 50px -22px color-mix(in srgb, var(--el-color-primary) 28.000000000000004%, transparent), 0 4px 12px #18202c14;
  font-family: 'Cascadia Code', 'SFMono-Regular', Consolas, monospace;
  font-size: 18px;
  font-weight: 400;
  font-variant-ligatures: none;
}
/* 全局重置直接设置文本元素的字体和字号，编码正文需要逐层继承窗口的等宽排版。 */
.code-window * { font-family: inherit; }
.code-body, .code-line, .source, .source *, .window-status > span { font-size: inherit; }
.window-bar { display: flex; align-items: center; gap: 18px; height: 7.5cqw; min-height: 34px; padding: 0 4.2%; border-bottom: 1px solid #ffffff0c; background: #ffffff03; }
.window-controls { display: flex; gap: 6px; flex-shrink: 0; }
.window-controls i { width: 9px; height: 9px; border-radius: 50%; background: #ff5f57; box-shadow: inset 0 0 0 1px #0000000a; }
.window-controls i:nth-child(2) { background: #febc2e; }
.window-controls i:nth-child(3) { background: #28c840; }
.file-name { display: flex; align-items: center; gap: 9px; font-size: 12px; color: #cbd3df; }
.language-badge { padding: 2px 5px; border-radius: 3px; background: #ffffff12; font-size: .85em; color: #d5e0f7; }
.file-mark { width: 15px; height: 15px; margin-left: auto; color: #7d8eaf; }
.code-body { padding: 2.5cqw 0; }
.code-line { display: flex; align-items: center; min-height: 4.4cqw; line-height: 1.65; white-space: pre; }
.code-section { margin-top: .65em; }
.line-number { width: 10%; padding-right: 3%; text-align: right; flex-shrink: 0; color: #99a9c6; font-size: 12px; user-select: none; }
.source { display: block; letter-spacing: 0; }
.source-indent { padding-left: 2ch; }
.syntax-keyword { color: var(--editor-blue); }
.syntax-type { color: #9dddeb; }
.syntax-method { color: var(--editor-method); }
.syntax-string { color: var(--editor-mint); }
.syntax-punctuation { color: #adbad5; }
.editor-caret { display: inline-block; width: 1px; height: 1.05em; margin-left: 4px; vertical-align: -.12em; background: #c8d6f1; }
.window-status { display: flex; align-items: center; gap: 7px; padding: 0 4.2%; height: 5cqw; min-height: 25px; border-top: 1px solid #ffffff09; font-size: 11px; color: #b4c3df; }
.status-indicator { height: 4px; width: 4px; background: #9bd3ba; border-radius: 50%; }
.file-encoding { margin-left: auto; color: #a5b6d5; }
:global(html.dark .code-scene) { --editor-surface: color-mix(in srgb, var(--el-color-primary) 20%, #131920); --editor-edge: color-mix(in srgb, var(--el-color-primary) 30%, #374352); }
:global(html.dark .code-window) { box-shadow: 0 24px 56px -20px #0008, 0 0 40px color-mix(in srgb, var(--el-color-primary) 4%, transparent); }
@container (max-width: 600px) { .code-window { font-size: 16px; } }
@container (max-width: 500px) {
  .scene-composition { aspect-ratio: 640 / 560; }
  .code-window { font-size: 12px; }
  .file-name, .window-status { font-size: 10px; }
  .line-number { font-size: 10px; }
  .window-bar { gap: 12px; }
  .window-controls { gap: 4px; }
  .window-controls i { width: 7px; height: 7px; }
}
@container (max-width: 400px) { .scene-composition { aspect-ratio: 640 / 600; } }
</style>
