﻿using SV20T1020570.DomainModels;

namespace SV20T1020570.Web.Models
{
    /// <summary>
    /// Lớp cha cho các lớp biểu diễn dữ liệu kết quả tìm kiếm, phân trang
    /// </summary>
    public abstract class BasePaginationResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValue { get; set; } = "";
        public int RowCount { get; set; }
        public int PageCount
        {
            get
            {
                if (PageSize == 0)
                    return 1;
                int c = RowCount / PageSize;
                if (RowCount % PageSize > 0)
                    c += 1;
                return c;
            }
        }
    }
}
