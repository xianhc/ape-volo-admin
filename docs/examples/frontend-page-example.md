# 岗位模块前端

文件路径相对于 `frontend/src/`。

| 职责 | 文件 |
| --- | --- |
| 请求封装 | [index.ts](../../frontend/src/api/organization/position/index.ts) |
| 类型 | [types.ts](../../frontend/src/api/organization/position/types.ts) |
| 列表与状态切换 | [index.vue](../../frontend/src/views/organization/position/index.vue) |
| 新增与编辑表单 | [formPanel.vue](../../frontend/src/views/organization/position/module/formPanel.vue) |
| CRUD 协调 | [UseCrud.ts](../../frontend/src/components/crud/UseCrud.ts) |
| 表单上下文 | [crudContext.ts](../../frontend/src/components/crud/crudContext.ts) |
| 状态字典 | [useDictionary.ts](../../frontend/src/composables/useDictionary.ts) 中的 `getDict`、`showDictLabel` |
| 用户岗位选择 | [用户管理页面](../../frontend/src/views/identity/user/index.vue) |

## 页面调用顺序

1. `index.vue` 创建查询状态，将 `get`、`add`、`edit`、`del` 和 `download` 绑定到 `useCrud`；新建表单默认名称为空、排序为 `999`、状态为启用。
2. `TablePage` 展示列表与分页，`SearchPanel` 提供名称、启用状态和日期范围查询，列定义支持排序；`CrudToolbarActions` 和 `RowActions` 触发新增、编辑、删除和导出。
3. `formPanel.vue` 通过 `useCrudForm<EntityDraft<Position>>()` 获取当前表单、抽屉和提交状态，调用 `validateAndSave` 完成前端校验与保存。
4. 页面读取 `status_type` 字典，供筛选、状态显示和表单单选项使用；表单通过 `strToBool` 将字典值转为布尔值。

页面组件名为 `Position`，通过 `defineOptions` 声明。按钮权限使用 `sys:position:add`、`sys:position:edit`、`sys:position:del` 和 `sys:position:download`。

请求封装使用 `/position/...` 路径，不重复添加 `/api`。列表与岗位选项通过 GET 查询，新增为 POST，编辑为 PUT，删除为 DELETE 并提交 `{ ids }`，导出为 POST 文件流。

## 状态切换与关联页面

列表中的启用开关由 `changeEnabled` 处理：设置该行加载状态，确认后调用 `edit(row)`；取消或请求失败时恢复原开关值，并清除加载状态。它复用 `/position/update`，没有单独的启停接口。

用户管理页面调用 `getAll` 取得启用岗位选项。调整岗位选项的返回字段或启用筛选时，需要同步核对用户管理页面的岗位选择与已分配岗位展示。

## 修改时联查

- 字段变更：先同步请求类型，再核对列表列定义、查询条件、`defaultForm`、表单输入与校验，并与 [后端 DTO 和映射](backend-module-example.md) 对齐。
- 列表操作：核对 `useCrud` 中 `crudMethod` 的绑定、分页排序参数和保存后的刷新行为。
- 状态切换：核对确认、取消、请求失败后的开关值和加载状态；该操作仍受后端编辑权限校验。
- 用户关联：核对 `getAll` 的使用方式，并检查已分配给用户的岗位删除时能否正确显示后端错误。
- 权限变更：核对页面按钮的权限标识、后端 `SystemPermissions.Positions` 和菜单权限配置，不能仅凭按钮隐藏判断接口已经授权。

执行 [前端检查](../frontend.md#检查)，并在浏览器中按受影响范围验证查询、分页排序、新增、编辑、删除、导出、状态切换与用户岗位选择；同时核对名称重复、排序不合法、岗位被用户引用和无权限时的界面表现。
