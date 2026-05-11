# LTSQL 单元测试说明

## 概述

本项目为 MNet.LTSQL 模块创建了全面的单元测试，覆盖了 LINQ to SQL 转换的核心功能。

## 测试文件清单

### 1. LTSQLBaseQueryTest.cs（已存在）
- 基础查询测试示例
- 展示了测试的基本结构和风格

### 2. LTSQLWhereQueryTest.cs
**测试内容：** WHERE 条件的各种场景
- 等于条件（`==`）
- 不等于条件（`!=`）
- 大于/小于条件（`>`, `<`, `>=`, `<=`）
- AND/OR 逻辑组合
- BETWEEN 范围查询
- LIKE 模糊匹配（Contains, StartsWith, EndsWith）
- NULL 值判断（IsNullOrEmpty）
- 复杂布尔表达式

**测试用例数量：** 15 个

### 3. LTSQLOrderByAndPagingTest.cs
**测试内容：** 排序和分页功能
- 单字段升序/降序排序
- 多字段排序
- ThenBy/ThenByDescending 链式排序
- Skip/Take 分页
- Distinct 去重
- 排序与分页组合

**测试用例数量：** 12 个

### 4. LTSQLGroupByAndAggregateTest.cs
**测试内容：** 分组和聚合函数
- Group By 基本分组
- 多字段分组
- 聚合函数：Count, Sum, Max, Min, Average, LongCount
- Having 条件过滤
- 分组与聚合组合
- Let 子句使用

**测试用例数量：** 14 个

### 5. LTSQLJoinQueryTest.cs
**测试内容：** 表连接操作
- Inner Join（内连接）
- Left Join（左外连接）
- Right Join（右外连接）
- 多表连接
- Join 与 Where 组合
- Join 与 Group By 组合
- SelectMany 扁平化查询

**测试用例数量：** 10 个

### 6. LTSQLInExistsSubQueryTest.cs
**测试内容：** IN、EXISTS 和子查询
- 简单 IN 操作（值列表）
- 字符串列表 IN 操作
- 元组形式 IN 操作
- 子查询 IN 操作
- NOT IN 操作
- EXISTS 操作
- NOT EXISTS 操作
- 相关子查询
- 标量子查询
- FROM 子句子查询
- 嵌套子查询
- IN 与子查询组合
- 复杂聚合子查询
- EXISTS 与 Join 组合
- 元组 IN 与子查询组合

**测试用例数量：** 15 个

### 7. LTSQLSetOperationTest.cs
**测试内容：** 集合操作
- UNION ALL（不去重）
- UNION DISTINCT（去重）
- INTERSECT（交集）
- EXCEPT（差集）
- 多个集合 Union
- 集合操作与 Where 组合
- Intersect 与 Group By 组合
- 复杂集合操作组合（Union + Intersect）
- Except 反向操作
- Distinct 标志对比

**测试用例数量：** 10 个

### 8. LTSQLSelectAndFunctionTest.cs
**测试内容：** SELECT 投影和函数
- 简单字段投影
- 字段重命名投影
- 计算字段投影
- 条件表达式投影（三元运算符）
- 字符串拼接
- 多重字符串拼接
- 日期时间函数（Year, Month, Day, Hour, Minute, Second）
- 多个日期时间函数组合
- 日期时间格式化
- 复杂投影（计算+条件+字符串）
- AsSelect 硬编码值查询
- 投影与 Where 组合
- 投影与 Group By 组合

**测试用例数量：** 17 个

### 9. LTSQLComplexQueryTest.cs
**测试内容：** 复杂查询综合场景
- 基础内连接 + 子查询 + 元组 IN
- 右外连接 + 左外连接 + 多维度 Group By + Having + Order By
- 多表 Join + 多重 Where + Group By + Having + Order By + 分页
- 嵌套子查询 + EXISTS + IN 组合
- Union + Intersect + Except 组合操作
- SelectMany + Join + Group By 组合
- 多层嵌套子查询
- 复杂聚合查询（多个聚合函数 + 分组 + 排序 + 分页）
- 自连接 + 递归查询模式
- 条件投影 + 动态排序
- 极端复杂查询（所有特性组合）

**测试用例数量：** 11 个

## 总计

- **测试文件数：** 9 个（其中 8 个为新创建）
- **测试用例总数：** 116 个
- **覆盖的功能点：**
  - 基础查询（WHERE, SELECT, FROM）
  - 排序（ORDER BY）
  - 分页（SKIP, TAKE）
  - 分组（GROUP BY, HAVING）
  - 聚合函数（COUNT, SUM, MAX, MIN, AVG）
  - 表连接（INNER JOIN, LEFT JOIN, RIGHT JOIN）
  - 集合操作（UNION, INTERSECT, EXCEPT）
  - 子查询（相关子查询、标量子查询、嵌套子查询）
  - IN/EXISTS 操作（包括元组形式）
  - 字符串函数（Contains, StartsWith, EndsWith）
  - 日期时间函数（Year, Month, Day, Hour, Minute, Second）
  - 条件表达式（三元运算符）
  - 字符串拼接
  - SelectMany 扁平化查询

## 技术栈

- **测试框架：** xUnit
- **数据访问：** Dapper
- **数据库：** SQLite (Microsoft.Data.Sqlite)
- **测试模型：** UnitTestModel 项目中的实体类

## 运行测试

```bash
cd Tests/LTSQLXUnitTest
dotnet test
```

运行特定测试类：
```bash
dotnet test --filter "FullyQualifiedName~LTSQLWhereQueryTest"
```

## 已知问题和限制

### 1. LTSQL 模块本身的限制

根据测试结果，发现以下 LTSQL 模块的限制：

1. **ToString() 方法暂不支持**：在表达式中使用 `ToString()` 会生成无效的 SQL（`no such function: ToString`）
   - **状态**：架构上支持扩展，当前未实现翻译逻辑
   - **解决方案**：避免在 LINQ 表达式中使用 ToString()，改用字符串字面量拼接
   - **未来计划**：可通过添加 FunctionCallToken 映射到 CAST(... AS TEXT) 或 || '' 实现

2. **ToUpper/Lower 等方法暂不支持**：类似的字符串处理方法尚未实现
   - **状态**：待实现功能
   - **未来计划**：可映射到 SQL 的 UPPER()/LOWER() 函数

3. **某些 GROUP BY 生成不完整**：部分聚合查询生成的 GROUP BY 子句为空
   - **影响**：导致 SQLite 语法错误
   - **建议**：简化分组逻辑或手动验证生成的 SQL

4. **GetEnumerator 未完全实现**：LTSQLObject.GetEnumerator 抛出 NotImplementedException
   - **影响**：在某些场景下无法直接调用 ToList()
   - **建议**：避免在子查询中直接使用 ToList()

5. **动态类型断言问题**：Dapper 返回的 dynamic 对象在 xUnit 断言时可能出现类型匹配问题
   - **解决方案**：进行显式类型转换后再断言

6. **SelectMany 相关子查询限制**：在 SQLite 环境下，SelectMany 中使用相关子查询无法生成有效 SQL
   - **原因**：SQLite 不支持 LATERAL JOIN 或 CROSS APPLY
   - **正确做法**：使用标准 Join 语法实现数据关联
   - **示例**：
     ```csharp
     // ❌ 错误：SelectMany + 相关子查询（无法翻译）
     persons.AsLTSQL().SelectMany(p => teachers.Where(t => t.PersionId == p.Id))
     
     // ✅ 正确：使用标准 Join
     from p in persons.AsLTSQL()
     join t in teachers.AsLTSQL().WithInner() on p.Id equals t.PersionId
     select ...
     ```

7. **GroupBy 后嵌套聚合限制**：在 GroupBy 后的 Select 中不能使用复杂的嵌套聚合表达式
   - **错误示例**：`g.Select(x => x.Field).Distinct().Count()`
   - **正确做法**：使用简单聚合函数 `g.Count()`、`g.Sum()` 等
   - **如需去重统计**：应通过 COUNT(DISTINCT field) 的形式由底层 SQL 支持

### 2. 测试设计建议

1. **参数空值检查**：ToSql 返回的参数列表可能为 null，使用前应做空值检查
2. **集合操作后需转换**：UnionSet/IntersectSet/ExceptSet 返回 ILTSQLObjectSetable，需要调用 AsLTSQL() 才能继续链式查询
3. **AsSelect 泛型约束**：必须使用具名类，不能使用匿名类型
4. **中文排序问题**：SQLite 的默认排序可能不支持中文的正确排序，建议使用数字字段测试排序功能

## 测试覆盖率

当前测试覆盖了 LTSQL 模块的主要功能点，包括：
- ✅ 基础 CRUD 查询
- ✅ 复杂条件组合
- ✅ 多表连接
- ✅ 聚合统计
- ✅ 子查询
- ✅ 集合操作
- ✅ 常用函数

## 后续改进建议

1. **增加边界条件测试**：空集合、NULL 值、极端数值等
2. **性能测试**：大数据量下的查询性能
3. **并发测试**：多线程环境下的线程安全性
4. **错误处理测试**：异常情况的容错性
5. **数据库兼容性测试**：扩展到 MySQL、PostgreSQL、SQL Server 等其他数据库

## 维护说明

- 所有测试都包含详细的中文注释
- 每个测试方法都有明确的测试目的说明
- 使用 ITestOutputHelper 输出 SQL 和结果，便于调试
- 遵循 AAA 模式（Arrange-Act-Assert）
- 测试数据使用真实的 SQLite 数据库，确保测试的真实性
