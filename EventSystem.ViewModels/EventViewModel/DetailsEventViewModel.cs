using EventSystem.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.ViewModels.EventViewModel
{
    //Идеята на този viewmodel е за да може за напред като ни потрябва евент с информация за него да го имаме наготово.
    //Тука не трябва никаква проверка, защото взимаме обектите от базата, където сме сигурно, че цялата информация е вярна.
    //Само когато се качва нещо към нея трябва проверка.
    //Думата required е същото все едно над него да напиша [Required] просто така кода изглежда по-четим. Той е от новата версия и по-стари го няма.
    public class DetailsEventViewModel
    {
        public required Guid id { get; set; }
        public required string Name { get; set; } = null!;
        public required string Description { get; set; } = null!;
        public required DateTime Date { get; set; }
        public required string Location { get; set; } = null!;
    }
}
