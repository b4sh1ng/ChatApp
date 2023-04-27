using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class SharedData
    {
        public static ICollectionView? FriendListCollection { get; set; }
    }
}
