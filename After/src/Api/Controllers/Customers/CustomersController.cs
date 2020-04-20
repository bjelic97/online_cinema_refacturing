using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Logic.Dtos;
using Logic.Entities;
using Logic.Repositories;
using Logic.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : BaseController
    {
        private readonly MovieRepository _movieRepository;
        private readonly CustomerRepository _customerRepository;

        public CustomersController(UnitOfWork unitOfWork, MovieRepository movieRepository, CustomerRepository customerRepository) : base(unitOfWork)
        {
            _customerRepository = customerRepository;
            _movieRepository = movieRepository;

        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(long id)
        {
            Customer customer = _customerRepository.GetById(id);
            if (customer == null)
                return NotFound();

            var dto = new CustomerDto
            {
                Name = customer.Name.Value,
                Email = customer.Email.Value,
                MoneySpent = customer.MoneySpent,
                Status = customer.Status.Type.ToString(),
                StatusExpirationDate = customer.Status.ExpirationDate,
                PurchasedMovies = customer.PurchasedMovies.Select(x => new PurchasedMovieDto
                {
                    Price = x.Price,
                    ExpirationDate = x.ExpirationDate,
                    Movie = new MovieDto
                    {
                        Name = x.Movie.Name
                    }
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet]
        public IActionResult GetList()
        {
            IReadOnlyList<Customer> customers = _customerRepository.GetList();

            List<CustomerInListDto> dtos = customers.Select(x => new CustomerInListDto
            {
                Name = x.Name.Value,
                Email = x.Email.Value,
                Status = x.Status.Type.ToString(),
                StatusExpirationDate = x.Status.ExpirationDate,
                MoneySpent = x.MoneySpent
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateCustomerDto item)
        {

            Result<CustomerName> customerNameOrError = CustomerName.Create(item.Name);
            Result<Email> emailOrError = Email.Create(item.Email);

            Result result = Result.Combine(customerNameOrError, emailOrError);

            if (result.IsFailure)
                return Error(result.Error);


            if (_customerRepository.GetByEmail(emailOrError.Value) != null)
                return Error("Email is already in use: " + item.Email);

            var customer = new Customer(customerNameOrError.Value, emailOrError.Value);

            _customerRepository.Add(customer);
            //  _customerRepository.SaveChanges();

            return Ok();


        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult Update(long id, [FromBody] UpdateCustomerDto item)
        {

            Result<CustomerName> customerNameOrError = CustomerName.Create(item.Name);

            if (customerNameOrError.IsFailure)
                return Error(customerNameOrError.Error);



            Customer customer = _customerRepository.GetById(id);
            if (customer == null)
                return Error("Invalid customer id: " + id);


            customer.Name = customerNameOrError.Value;
            //  _customerRepository.SaveChanges();

            return Ok();

        }

        [HttpPost]
        [Route("{id}/movies")]
        public IActionResult PurchaseMovie(long id, [FromBody] long movieId)
        {

            Movie movie = _movieRepository.GetById(movieId);
            if (movie == null)
                return Error("Invalid movie id: " + movieId);


            Customer customer = _customerRepository.GetById(id);
            if (customer == null)
                return Error("Invalid customer id: " + id);

            if (customer.HasPurchasedMovie(movie))
                return Error("The movie is already purchased: " + movie.Name);


            customer.PurchaseMovie(movie);

            //  _customerRepository.SaveChanges();

            return Ok();

        }

        [HttpPost]
        [Route("{id}/promotion")]
        public IActionResult PromoteCustomer(long id)
        {

            Customer customer = _customerRepository.GetById(id);
            if (customer == null)
                return Error("Invalid customer id: " + id);

            Result promotionCheck = customer.CanPromote();

            if (promotionCheck.IsFailure)
                return Error(promotionCheck.Error);

            customer.Promote();
            //  _customerRepository.SaveChanges();

            return Ok();

        }
    }
}
