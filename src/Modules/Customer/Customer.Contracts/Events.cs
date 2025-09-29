using System;

namespace Customer.Contracts;

public record CustomerRegistered(Guid CustomerId, string Email);
