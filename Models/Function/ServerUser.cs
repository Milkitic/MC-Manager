using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gm.Models.Function
{
    public class ServerUser
    {
        public int ID { get; set; }
        public DateTime Time { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }

        // 扩展
        public string ServerTime { get; set; }
        public string TimeCountDown { get; set; }

        public ServerUser(int id, DateTime time, string user, string comment)
        {
            ID = id;
            Time = time;
            UserName = user;
            Comment = comment;
        }
    }
}
