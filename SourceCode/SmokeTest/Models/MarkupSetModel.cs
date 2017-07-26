using System;

namespace SmokeTest.Models
{
    public class MarkupSetModel
    {
        public string Name { get; set; }
        public int MarkupSetOrder { get; set; }
        public string RedactionText { get; set; }

        public MarkupSetModel(string name, int markupSetOrder, string redactionText)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (redactionText == null)
            {
                throw new ArgumentNullException(nameof(redactionText));
            }

            Name = name;
            MarkupSetOrder = markupSetOrder;
            RedactionText = redactionText;
        }
    }
}
