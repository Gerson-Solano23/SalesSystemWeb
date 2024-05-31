﻿using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface ISale
    {
        Task<List<SaleDTO>> List();
        Task<SaleDTO> Create(SaleDTO entity);

    }
}
