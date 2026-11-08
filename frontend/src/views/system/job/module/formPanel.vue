<template>
  <el-drawer v-model="dialogVisible" :size="appStore.drawerSize" :before-close="closeDialog" :show-close="false">
    <template #header>
      <div class="job-editor-header">
        <span>{{ getFormTitle() }}</span>
        <div>
          <el-button :disabled="loading" @click="closeDialog">取消</el-button>
          <el-button type="primary" :loading="loading" @click="handleSave">保存</el-button>
        </div>
      </div>
    </template>
    <p class="job-editor-note">启用的作业保存后会按当前配置重新调度。仅保存配置时，请关闭“启用配置”。</p>
    <el-form ref="jobForm" :model="form" :rules="dynamicRules" :validate-on-rule-change="false" label-position="top" class="job-editor-grid">
      <el-form-item label="作业名称" prop="jobName">
        <el-input v-model="form.jobName" placeholder="为作业设置名称" />
      </el-form-item>
      <el-form-item label="分组" prop="jobGroup">
        <el-input v-model="form.jobGroup" placeholder="例如：系统维护" />
      </el-form-item>
      <el-form-item label="触发方式" prop="triggerType" class="job-editor-wide">
        <el-radio-group v-model="form.triggerType" @change="handleModeChange">
          <el-radio-button :value="1">Cron 表达式</el-radio-button>
          <el-radio-button :value="2">固定间隔</el-radio-button>
        </el-radio-group>
      </el-form-item>
      <el-form-item v-if="form.triggerType === 1" label="Cron 表达式" prop="cron" class="job-editor-wide">
        <el-input v-model="form.cron" placeholder="例如：0 0/1 * * * ?" />
        <p class="job-field-hint">使用“秒 分 时 日 月 周”格式；示例表示每分钟触发一次。</p>
      </el-form-item>
      <template v-if="form.triggerType === 2">
        <el-form-item label="执行间隔（秒）" prop="intervalSecond">
          <el-input-number v-model="form.intervalSecond" :min="6" :precision="0" controls-position="right" />
          <p class="job-field-hint">每次触发间隔，至少 6 秒。</p>
        </el-form-item>
        <el-form-item label="执行次数" prop="cycleRunTimes">
          <el-input-number v-model="form.cycleRunTimes" :min="0" :precision="0" controls-position="right" />
          <p class="job-field-hint">0 表示持续触发；填写 10 表示总共触发 10 次。</p>
        </el-form-item>
      </template>
      <el-form-item label="程序集" prop="assemblyName">
        <el-input v-model="form.assemblyName" placeholder="例如：Ape.Volo.Web" />
      </el-form-item>
      <el-form-item label="执行类" prop="className">
        <el-input v-model="form.className" placeholder="填写作业类的完整名称" />
      </el-form-item>
      <el-form-item label="开始时间（可选）" prop="startTime">
        <el-date-picker v-model="form.startTime" type="datetime" value-format="YYYY-MM-DD HH:mm:ss" placeholder="不限制开始时间" clearable />
      </el-form-item>
      <el-form-item label="结束时间（可选）" prop="endTime">
        <el-date-picker v-model="form.endTime" type="datetime" value-format="YYYY-MM-DD HH:mm:ss" placeholder="不限制结束时间" clearable />
      </el-form-item>
      <el-form-item label="负责人（可选）" prop="principal">
        <el-input v-model="form.principal" />
      </el-form-item>
      <el-form-item label="告警邮箱（可选）" prop="alertEmail">
        <el-input v-model="form.alertEmail" placeholder="多个邮箱用逗号分隔" />
      </el-form-item>
      <el-form-item label="启用配置" prop="enabled">
        <el-switch v-model="form.enabled" inline-prompt active-text="启用" inactive-text="停用" />
      </el-form-item>
      <el-form-item label="失败后暂停" prop="pauseAfterFailure">
        <el-switch v-model="form.pauseAfterFailure" inline-prompt active-text="是" inactive-text="否" />
      </el-form-item>
      <el-form-item label="执行参数（可选）" prop="runParams" class="job-editor-wide">
        <el-input v-model="form.runParams" :rows="3" type="textarea" placeholder="填写传递给作业的参数" />
      </el-form-item>
      <el-form-item label="描述（可选）" prop="description" class="job-editor-wide">
        <el-input v-model="form.description" :rows="3" type="textarea" />
      </el-form-item>
    </el-form>
  </el-drawer>
</template>

<script setup lang="ts">
  import { computed, ref } from 'vue'
  import type { FormInstance, FormRules } from 'element-plus'
  import { useCrudForm } from '@/components/crud/crudContext'
  import type { EntityDraft } from '@/api/types/base'
  import type { Job } from '@/api/system/types/job.types'
  import { useAppStore } from '@/pinia'

  const appStore = useAppStore()
  const crud = useCrudForm<EntityDraft<Job>>()
  const { form, dialogVisible, loading, closeDialog, validateAndSave, getFormTitle } = crud
  const jobForm = ref<FormInstance>()
  const handleModeChange = () => {
    if (form.value.triggerType === 1) {
      form.value.intervalSecond = null
      form.value.cycleRunTimes = null
    } else if (form.value.triggerType === 2) {
      form.value.cron = null
      form.value.intervalSecond ??= 60
      form.value.cycleRunTimes ??= 0
    }
    jobForm.value?.clearValidate(['cron', 'intervalSecond', 'cycleRunTimes'])
  }
  const dynamicRules = computed<FormRules>(() => ({
    jobName: [{ required: true, whitespace: true, message: '请输入作业名称', trigger: 'blur' }],
    jobGroup: [{ required: true, whitespace: true, message: '请输入分组', trigger: 'blur' }],
    assemblyName: [{ required: true, whitespace: true, message: '请输入程序集', trigger: 'blur' }],
    className: [{ required: true, whitespace: true, message: '请输入执行类', trigger: 'blur' }],
    triggerType: [{ required: true, type: 'enum', enum: [1, 2], message: '请选择触发方式', trigger: 'change' }],
    ...(form.value.triggerType === 1 ? {
      cron: [{ required: true, whitespace: true, message: '请输入 Cron 表达式', trigger: 'blur' }]
    } : form.value.triggerType === 2 ? {
      intervalSecond: [{ required: true, type: 'number', min: 6, message: '执行间隔至少为 6 秒', trigger: 'change' }]
    } : {}),
    endTime: [{
      validator: (_rule, value, callback) => {
        if (value && form.value.startTime && new Date(value).getTime() <= new Date(form.value.startTime).getTime()) {
          callback(new Error('结束时间必须晚于开始时间'))
        } else callback()
      },
      trigger: 'change'
    }]
  }))
  const handleSave = () => validateAndSave(jobForm.value)
</script>

<style scoped>
  .job-editor-header { display: flex; align-items: center; justify-content: space-between; gap: 16px; }
  .job-editor-note { margin: 0 0 22px; color: var(--el-text-color-secondary); font-size: 13px; line-height: 1.6; }
  .job-editor-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 0 20px; }
  .job-editor-wide { grid-column: 1 / -1; }
  .job-editor-grid :deep(.el-input-number), .job-editor-grid :deep(.el-date-editor) { width: 100%; }
  .job-field-hint { width: 100%; margin: 6px 0 0; color: var(--el-text-color-secondary); font-size: 12px; line-height: 1.5; }
  @media (max-width: 767px) { .job-editor-grid { grid-template-columns: minmax(0, 1fr); } }
</style>
