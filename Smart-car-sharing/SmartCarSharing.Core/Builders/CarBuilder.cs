using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCarSharing.Core.Builders
{
    public class CarBuilder
    {
        private Car _car;

        public CarBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            _car = new Car
            {
                Id = 0,
            };
        }

        public CarBuilder WithModel(string make, string model)
        {
            _car.Make = make;
            _car.Model = model;
            return this;
        }

        public CarBuilder WithYear(int year)
        {
            _car.Year = year;
            return this;
        }

        public CarBuilder WithPrice(decimal pricePerHour)
        {
            _car.PricePerHour = pricePerHour;
            return this;
        }

        public CarBuilder WithLocation(string location)
        {
            _car.Location = location;
            return this;
        }

        public Car Build()
        {
            Car result = _car;
            Reset();
            return result;
        }
    }
}
