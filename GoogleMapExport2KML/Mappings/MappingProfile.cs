using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GoogleMapExport2KML.Extensions;
using GoogleMapExport2KML.Models;

namespace GoogleMapExport2KML.Mappings;
internal class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CsvLineItem, Placemark>().ConvertUsing((csv, plc, ctx) =>
        {
            ctx.Items.TryGetValue<bool>("IncludeDescription", out var includeComment);

        });
    }
}
