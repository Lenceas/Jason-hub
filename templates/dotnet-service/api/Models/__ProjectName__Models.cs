using System.ComponentModel;
using System.Text.Json.Serialization;

namespace __ProjectName__Api.Models;

/// <summary>
/// 请求/响应 DTO 定义在此文件。
///
/// 规范：
/// - 使用 record 类型定义 DTO
/// - 每条记录必须加 <![CDATA[/// <summary>]]> 文档注释
/// - 每个字段必须加 [property: Description("字段说明")]（生成 OpenAPI Schema 描述）
/// - 每个字段必须加 [property: JsonPropertyName("snake_case")]（JSON 序列化名）
/// - 必填字段不加 ?，可选字段加 ?
/// - 每个字段独占一行，属性注解在上方
/// </summary>

// ======== 请求 DTO 示例 ========

///// <summary>示例请求</summary>
//public record ExampleRequest(
//    [property: Description("用户名")]
//    [property: JsonPropertyName("user_name")]
//    string UserName,

//    [property: Description("电子邮箱")]
//    [property: JsonPropertyName("email")]
//    string Email,

//    [property: Description("年龄（可选）")]
//    [property: JsonPropertyName("age")]
//    int? Age
//);

// ======== 响应 DTO 示例 ========

///// <summary>示例响应</summary>
//public record ExampleResponse(
//    [property: Description("用户ID")]
//    [property: JsonPropertyName("id")]
//    int Id,

//    [property: Description("创建时间")]
//    [property: JsonPropertyName("created_at")]
//    DateTime CreatedAt
//);
