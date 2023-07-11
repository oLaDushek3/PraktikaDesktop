using Avalonia.Data.Converters;
using PraktikaDesktop.Models;
using System;
using System.Globalization;

namespace PraktikaDesktop.Converters
{
    public class ProductPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                SupplyProduct supplyProduct = value as SupplyProduct;
                decimal? price = 0;

                if (supplyProduct.Product.Price != null)
                {
                    price = supplyProduct.Product.Price;
                }
                else
                {
                    foreach (ProductPriceCategory priceCategory in supplyProduct.Product.ProductPriceCategories)
                    {
                        if (priceCategory.PriceCategoryId == supplyProduct.Textile.PriceCategoryId)
                            price = priceCategory.Price;
                    }
                }
                return price.ToString();
            }
            else
                return value;

        }

        public static decimal Convert(SupplyProduct supplyProduct)
        {
            decimal price = 0;

            if (supplyProduct.Product.Price != null)
            {
                price = (decimal)supplyProduct.Product.Price;
            }
            else
            {
                foreach (ProductPriceCategory priceCategory in supplyProduct.Product.ProductPriceCategories)
                {
                    if (priceCategory.PriceCategoryId == supplyProduct.Textile.PriceCategoryId)
                        price = priceCategory.Price;
                }
            }
            return price;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
