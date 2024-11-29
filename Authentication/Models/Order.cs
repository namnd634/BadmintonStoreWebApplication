using System;
using System.Collections.Generic;

namespace Authentication.Models;

public partial class Order
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string? Status { get; set; }

    public string? CustomerId { get; set; }

    public string? EmployeeId { get; set; }

    public virtual AspNetUser? Customer { get; set; }

    public virtual AspNetUser? Employee { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
