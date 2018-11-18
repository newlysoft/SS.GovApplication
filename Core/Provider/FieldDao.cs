﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using SiteServer.Plugin;
using SS.GovApplication.Core.Model;

namespace SS.GovApplication.Core.Provider
{
  public static class FieldDao
  {
    public const string TableName = "ss_gov_application_field";

    public static List<TableColumn> Columns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Id),
                DataType = DataType.Integer,
                IsIdentity = true,
                IsPrimaryKey = true
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.SiteId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Taxis),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Title),
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Description),
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.PlaceHolder),
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.FieldType),
                DataType = DataType.VarChar,
                DataLength = 50
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Validate),
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = nameof(FieldInfo.Settings),
                DataType = DataType.Text
            }
        };

    public static int Insert(int siteId, FieldInfo fieldInfo)
    {
      fieldInfo.Taxis = GetMaxTaxis(fieldInfo.SiteId) + 1;

      var sqlString = $@"INSERT INTO {TableName}
(
    {nameof(FieldInfo.SiteId)}, 
    {nameof(FieldInfo.Taxis)},
    {nameof(FieldInfo.Title)},
    {nameof(FieldInfo.Description)},
    {nameof(FieldInfo.PlaceHolder)},
    {nameof(FieldInfo.FieldType)},
    {nameof(FieldInfo.Validate)},
    {nameof(FieldInfo.Settings)}
) VALUES (
    @{nameof(FieldInfo.SiteId)}, 
    @{nameof(FieldInfo.Taxis)},
    @{nameof(FieldInfo.Title)},
    @{nameof(FieldInfo.Description)},
    @{nameof(FieldInfo.PlaceHolder)},
    @{nameof(FieldInfo.FieldType)},
    @{nameof(FieldInfo.Validate)},
    @{nameof(FieldInfo.Settings)}
)";

      var parameters = new[]
{
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.SiteId), fieldInfo.SiteId),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Taxis), fieldInfo.Taxis),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Title), fieldInfo.Title),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Description), fieldInfo.Description),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.PlaceHolder), fieldInfo.PlaceHolder),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.FieldType), fieldInfo.FieldType),
          Context.DatabaseApi.GetParameter(nameof(FieldInfo.Validate), fieldInfo.Validate),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Settings), fieldInfo.Settings)
            };

      var id = Context.DatabaseApi.ExecuteNonQueryAndReturnId(TableName, nameof(FieldInfo.Id), Context.ConnectionString, sqlString, parameters);

      ItemDao.InsertItems(fieldInfo.SiteId, id, fieldInfo.Items);

      FieldManager.ClearCache();
      return id;
    }

    public static void Update(FieldInfo info, bool updateItems)
    {
      var sqlString = $@"UPDATE {TableName} SET
                {nameof(FieldInfo.SiteId)} = @{nameof(FieldInfo.SiteId)}, 
                {nameof(FieldInfo.Taxis)} = @{nameof(FieldInfo.Taxis)}, 
                {nameof(FieldInfo.Title)} = @{nameof(FieldInfo.Title)},
                {nameof(FieldInfo.Description)} = @{nameof(FieldInfo.Description)},
                {nameof(FieldInfo.PlaceHolder)} = @{nameof(FieldInfo.PlaceHolder)},
                {nameof(FieldInfo.FieldType)} = @{nameof(FieldInfo.FieldType)},
                {nameof(FieldInfo.Validate)} = @{nameof(FieldInfo.Validate)},
                {nameof(FieldInfo.Settings)} = @{nameof(FieldInfo.Settings)}
            WHERE {nameof(FieldInfo.Id)} = @{nameof(FieldInfo.Id)}";

      var updateParms = new[]
{
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.SiteId), info.SiteId),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Taxis), info.Taxis),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Title), info.Title),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Description), info.Description),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.PlaceHolder), info.PlaceHolder),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.FieldType), info.FieldType),
          Context.DatabaseApi.GetParameter(nameof(FieldInfo.Validate), info.Validate),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Settings), info.Additional.ToString()),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Id), info.Id)
            };

      Context.DatabaseApi.ExecuteNonQuery(Context.ConnectionString, sqlString, updateParms);

      if (updateItems)
      {
        ItemDao.DeleteByFieldId(info.Id);
        ItemDao.InsertItems(info.SiteId, info.Id, info.Items);
      }

      FieldManager.ClearCache();
    }

    public static void Delete(int fieldId)
    {
      string sqlString = $"DELETE FROM {TableName} WHERE {nameof(FieldInfo.Id)} = @{nameof(FieldInfo.Id)}";

      var parms = new[]
      {
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Id), fieldId)
            };

      Context.DatabaseApi.ExecuteNonQuery(Context.ConnectionString, sqlString, parms);

      ItemDao.DeleteByFieldId(fieldId);

      FieldManager.ClearCache();
    }

    public static void DeleteBySiteId(int siteId)
    {
      string sqlString = $"DELETE FROM {TableName} WHERE {nameof(FieldInfo.SiteId)} = @{nameof(FieldInfo.SiteId)}";

      var parms = new[]
      {
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.SiteId), siteId)
            };

      Context.DatabaseApi.ExecuteNonQuery(Context.ConnectionString, sqlString, parms);

      ItemDao.DeleteBySiteId(siteId);

      FieldManager.ClearCache();
    }

    public static bool IsTitleExists(int siteId, string title)
    {
      var exists = false;

      string sqlString = $@"SELECT Id FROM {TableName} WHERE 
    {nameof(FieldInfo.SiteId)} = @{nameof(FieldInfo.SiteId)} AND 
    {nameof(FieldInfo.Title)} = @{nameof(FieldInfo.Title)}";

      var parms = new[]
{
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.SiteId), siteId),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Title), title)
            };

      using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString, parms))
      {
        if (rdr.Read() && !rdr.IsDBNull(0))
        {
          exists = true;
        }
        rdr.Close();
      }

      return exists;
    }

    private static int GetMaxTaxis(int siteId)
    {
      string sqlString =
          $"SELECT MAX(Taxis) AS MaxTaxis FROM {TableName} WHERE {nameof(FieldInfo.SiteId)} = {siteId}";
      var maxTaxis = 0;

      using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString))
      {
        if (rdr.Read() && !rdr.IsDBNull(0))
        {
          maxTaxis = rdr.GetInt32(0);
        }
        rdr.Close();
      }
      return maxTaxis;
    }

    public static void TaxisDown(int id)
    {
      var fieldInfo = FieldManager.GetFieldInfo(id);
      if (fieldInfo == null) return;

      var sqlString = Context.DatabaseApi.GetPageSqlString(TableName, "Id, Taxis", $"WHERE {nameof(FieldInfo.SiteId)} = @{nameof(FieldInfo.SiteId)} AND Taxis > (SELECT Taxis FROM {TableName} WHERE Id = @Id)", "ORDER BY Taxis", 0, 1);

      var higherId = 0;
      var higherTaxis = 0;

      var parms = new[]
      {
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.SiteId), fieldInfo.SiteId),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Id), id)
            };

      using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString, parms))
      {
        if (rdr.Read() && !rdr.IsDBNull(0))
        {
          higherId = rdr.GetInt32(0);
          higherTaxis = rdr.GetInt32(1);
        }
        rdr.Close();
      }

      if (higherId != 0)
      {
        SetTaxis(id, higherTaxis);
        SetTaxis(higherId, fieldInfo.Taxis);
      }
    }

    public static void TaxisUp(int id)
    {
      var fieldInfo = FieldManager.GetFieldInfo(id);
      if (fieldInfo == null) return;

      var sqlString = Context.DatabaseApi.GetPageSqlString(TableName, "Id, Taxis", $"WHERE {nameof(FieldInfo.SiteId)} = @{nameof(FieldInfo.SiteId)} AND Taxis < (SELECT Taxis FROM {TableName} WHERE Id = @Id)", "ORDER BY Taxis DESC", 0, 1);
      var lowerId = 0;
      var lowerTaxis = 0;

      var parms = new[]
      {
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.SiteId), fieldInfo.SiteId),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Id), id)
            };

      using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString, parms))
      {
        if (rdr.Read() && !rdr.IsDBNull(0))
        {
          lowerId = rdr.GetInt32(0);
          lowerTaxis = rdr.GetInt32(1);
        }
        rdr.Close();
      }

      if (lowerId != 0)
      {
        SetTaxis(id, lowerTaxis);
        SetTaxis(lowerId, fieldInfo.Taxis);
      }
    }

    private static void SetTaxis(int id, int taxis)
    {
      var sqlString = $"UPDATE {TableName} SET Taxis = @Taxis WHERE Id = @Id";

      var parms = new[]
{
        Context.DatabaseApi.GetParameter(nameof(FieldInfo.Taxis), taxis),
                Context.DatabaseApi.GetParameter(nameof(FieldInfo.Id), id)
      };

      Context.DatabaseApi.ExecuteNonQuery(Context.ConnectionString, sqlString, parms);

      FieldManager.ClearCache();
    }

    private static FieldInfo GetFieldInfo(IDataRecord rdr)
    {
      if (rdr == null) return null;

      var fieldInfo = new FieldInfo();

      var i = 0;
      fieldInfo.Id = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
      i++;
      fieldInfo.SiteId = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
      i++;
      fieldInfo.Taxis = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
      i++;
      fieldInfo.Title = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);
      i++;
      fieldInfo.Description = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);
      i++;
      fieldInfo.PlaceHolder = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);
      i++;
      fieldInfo.FieldType = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);
      i++;
      fieldInfo.Validate = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);
      i++;
      fieldInfo.Settings = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);

      return fieldInfo;
    }

    public static List<KeyValuePair<string, FieldInfo>> GetAllFieldInfoList()
    {
      var pairs = new List<KeyValuePair<string, FieldInfo>>();

      var allItemsDict = ItemDao.GetAllItems();

      using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, $@"SELECT
    {nameof(FieldInfo.Id)}, 
    {nameof(FieldInfo.SiteId)}, 
    {nameof(FieldInfo.Taxis)},
    {nameof(FieldInfo.Title)},
    {nameof(FieldInfo.Description)},
    {nameof(FieldInfo.PlaceHolder)},
    {nameof(FieldInfo.FieldType)},
    {nameof(FieldInfo.Validate)},
    {nameof(FieldInfo.Settings)}
FROM {TableName} 
ORDER BY {nameof(FieldInfo.Taxis)} DESC, {nameof(FieldInfo.Id)} DESC"))
      {
        while (rdr.Read())
        {
          var fieldInfo = GetFieldInfo(rdr);

          allItemsDict.TryGetValue(fieldInfo.Id, out var items);
          if (items == null)
          {
            items = new List<ItemInfo>();
          }
          fieldInfo.Items = items;
          fieldInfo.Additional = new FieldSettings(fieldInfo.Settings);

          var key = FieldManager.GetKey(fieldInfo.SiteId, fieldInfo.Title);

          if (pairs.All(pair => pair.Key != key))
          {
            var pair = new KeyValuePair<string, FieldInfo>(key, fieldInfo);
            pairs.Add(pair);
          }
        }
        rdr.Close();
      }

      return pairs;
    }
  }
}
