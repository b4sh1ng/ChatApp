using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    class OnlineFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }

        public OnlineFriendsView()
        {
            FriendList = new();
            for (int i = 0; i < 2; i++)
            {
                FriendList.Add(new FriendModel()
                {
                    FriendId = 1,
                    Username = "Preacher",
                    UsernameId = 1,
                    ImageSource = "UklGRgQFAABXRUJQVlA4IPgEAACQHQCdASqAAIAAPm0wlUikIqIhI5kqYIANiWkKFshYsipAzvVP/n/ogDe5bcR/pPaz/PPyA67jwP6+ciGIp8Y+232/h92o/7Z+WnA78X/n/+J+3XnA+U/8jvc9/1X5d++H+c/pPkE0AP47/VP9p/WfyA+Ov/F8vX0F/0fcD/lv9X/5PYs9GAiTBt1VeT5dSEMZw0bITdQJzQQqfFxs/dOVh8gDHAxrpZIjyDtTsfRwGyjfRi8CLUZAvbXAE3jsx4VnbmkFitQRICEn/7xRYGaq6EPveEY4OTfnSoYBEzIGdwOVCkg2ymrme/qkXvAidF1IRaeqAAD+/6lGAA9+j3MVmMf+QcwxJiOSXxlK8mUpJ91HxcetU0C55eWHFyNgsal4jzlWZR6u2+Vekm2rO/M5emKSO1WqGoBTifg2P3CzKAyfkF3wvnxMbF32J5Ylj2c4USSpbwqyMngkro/0ZrEgbb9VVcA0mTTdgnOSWZwW9jGY9oCEc//tjPIZ+yCh0fSaf9scjN0ALIL2rpD/BIzSeaZ++Dxiou3OqK/Wd5b2FaAVl++TmW23Nh7fmOzQPw7qgkQ0XNOnrLULY41+LgAqYJFlAjwObB//E81I1icMQ9urKkxvcnoZbRqyXUMYBcgPoZcn6NHZI6EsAh3A+50QBSZCWRJezBeXB7JGrigvD64E3ZEdMqMGpq713AWInUUSOkJ+6XsVo4mGoa2I4kcsHJs1HcmRpEeuEbVwjVRoKlp/pdG3PF7Hl64OAtHXyg30K1KkLP2Xc8bj0kCCCzeirH24Gmv5E+v1wyC0wQF9Fu+YSlNby0R8nhDcX2CODBvCqg8y9EPtTvIerMep78A1nUsz//DLIngzdrsRzn9TZR4xshU3HTgNkFlsjQNmAgLHnn6q6A9wo5C3/aZ/6jO7EpwHwI9sziv/yALhmd/g+zw+Ui/4Oq7pcXKw8wh55/AS3iNsR66dOkcu7OiV+YR13SrUk7HnlrHXdMe/14lEoKAblBcaxmNCH6//86DlPrQE77RuSksFjoLf/wwDeYJ8TLi2dGG5Ht2uatvB54o2EE7WcIhCyZ6cRq3R6OsNVGv1oOX5QN6wjt2zBJKZDrCTMkCiHQeUS1tBBKa2OB37wFnBroE44bMdQMnAS/ao1msfkIzNwOv3z5WsGLgZMLGxQzE3Orknw+VwD/DJ53CM0gwL/H/9p7YkB2++bpk2un//DY8/yxAVRJRZTA54gGmNv26P8NlHC3dJRREXvXhOog9H/pIvFCsXEbXGEEdPZvb/0Bi3YGBl2lY9S/cwRFV014hrLyR6pisIIJ6orfSD8/kbJvHEZiJq/BK/qBrXXg+nJN8iyaI7k8AOXLnPNlIT/J5S67IbEQk6We8qNKfNdSnb1yU3z22YVQFuaILYQK4i7A7GnbNJuAbyn6P/9knJrwVyO2zBNEeNRDd30mCAolZ1eZ2WDpS/x+hWLyrOjGzKmeUdegLjhB4Lv73FNhwwoqz/Pirb8FUJiqy0P3eVpkjdhJCsdSsFKuAwz935slnNEG5VUqZ3mfgwPIF+bOx9R8QpWgRvyS1wrn/QxBoQUiBcqlWwqZ6VqOQHnU+iPi2PmQN0s9gPt667DVMfkskx9CwJLcWX+SQhACYUX+QMBnjJEvvcJ101Armdyg3aTqI6B0k7kYJgpkJwP0S9PJnwz7G30AAAAAA=",
                    IsFriend = true,
                });
            }
            
        }
    }
}
