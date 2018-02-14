using gm.Functions;
using gm.Models.Function;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace gm.BLL
{
    public class BLL_Chat
    {
        public int InsertChat(string user, string message)
        {
            return DataBase.DbHelper.ExecuteNonQuery("INSERT INTO tbl_chat (time, user, message, new) VALUES (@date, @user, @message, 1)",
                new MySqlParameter("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new MySqlParameter("@user", user),
                new MySqlParameter("@message", message));
        }
        public int MarkChatById(int id)
        {
            return DataBase.DbHelper.ExecuteNonQuery("UPDATE tbl_chat SET new = 0 WHERE id = @id",
                new MySqlParameter("@id", id));
        }
        public int MarkAllChat()
        {
            return DataBase.DbHelper.ExecuteNonQuery("UPDATE tbl_chat SET new = 0 WHERE new = 1");
        }
        public List<ServerUser> GetChatListOrderByTime(int index, int length, bool isNew = true)
        {
            return _GetChatList($"SELECT * FROM tbl_chat{(isNew == true ? " WHERE new = 1" : "")} ORDER BY time DESC LIMIT {index},{length}");
        }

        public List<ServerUser> GetAllChatListOrderByTime(bool isNew = true)
        {
            return _GetChatList($"SELECT * FROM tbl_chat{(isNew == true ? " WHERE new = 1" : "")} ORDER BY time DESC");
        }

        private List<ServerUser> _GetChatList(string queryString, params MySqlParameter[] param)
        {
            List<ServerUser> parsed_list = new List<ServerUser>();
            DataTable dataTable = DataBase.DbHelper.GetResult(queryString, param);
            foreach (DataRow item in dataTable.Rows)
            {
                parsed_list.Add(new ServerUser(Convert.ToInt32(item["id"]), DateTime.Parse(item["time"].ToString()), item["user"].ToString(), item["message"].ToString()));
            }
            foreach (var obj in parsed_list)
            {
                string timeCountDown;
                var now = DateTime.Now;
                var ts = (now - obj.Time).TotalSeconds;
                if (ts < 60)
                {
                    ts = (int)(ts);
                    timeCountDown = $"{ts} 秒前";
                }
                else if (ts < 60 * 60)
                {
                    ts = (int)(ts / 60);
                    timeCountDown = $"{ts} 分钟前";
                }
                else if (ts < 60 * 60 * 24)
                {
                    ts = (int)(ts / 60 / 60);
                    timeCountDown = $"{ts} 小时前";
                }
                else if (ts < 60 * 60 * 24 * 30)
                {
                    ts = (int)(ts / 60 / 60 / 24);
                    timeCountDown = $"{ts} 天前";
                }
                else if (ts < 60 * 60 * 24 * 30 * 12)
                {
                    ts = (int)(ts / 60 / 60 / 24 / 30);
                    timeCountDown = $"{ts} 个月前";
                }
                else
                {
                    ts = (int)(ts / 60 / 60 / 24 / 30 / 12);
                    timeCountDown = $"{ts} 年前";
                }
                obj.TimeCountDown = timeCountDown;
                obj.ServerTime = obj.Time.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return parsed_list;
        }
    }
}
