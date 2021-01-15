using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerCrawler
{
    class News
    {
        public int Id { get; set; }
        public string UrlBasePost { get; set; }
        public string Title { get; set; }
        public string UrlImg { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
    }
}
