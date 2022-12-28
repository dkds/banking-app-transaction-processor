using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionProcessor.Models;

namespace TransactionProcessor.Data
{
    public class TransactionProcessorContext : DbContext
    {
        public TransactionProcessorContext (DbContextOptions<TransactionProcessorContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transaction { get; set; } = default!;
    }
}

