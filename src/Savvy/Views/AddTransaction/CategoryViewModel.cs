using Caliburn.Micro;
using YnabApi.Items;

namespace Savvy.Views.AddTransaction
{
    public class CategoryViewModel : PropertyChangedBase
    {
        public CategoryViewModel(MasterCategory masterCategory, Category category)
        {
            this.MasterCategory = masterCategory;
            this.Category = category;

            this.Name = $"{masterCategory.Name} - {category.Name}";
        }

        public MasterCategory MasterCategory { get; }
        public Category Category { get; }

        public string Name { get; }
    }
}