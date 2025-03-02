using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace seth
{
    public enum Heart {RED, ORANGE, YELLOW, GREEN, BLUE, PURPLE};
    public class Player
    {
        public Player(SocketUser user)
        {
            this.user = user;
        }     
        public SocketUser user;
        public int pebbles = 0;
        public List<Heart> Hearts = new List<Heart>();
        public JsonObject Save() {
            JsonObject json = new JsonObject();
            json.Add("id", user.Id);
            json.Add("pebbles", pebbles);
            JsonArray hearts_array = new JsonArray();
            foreach (Heart heart in Hearts)
            {
                hearts_array.Add(heart.ToString());
            }
            json.Add("hearts", hearts_array);
            return json;
        }
    }
}