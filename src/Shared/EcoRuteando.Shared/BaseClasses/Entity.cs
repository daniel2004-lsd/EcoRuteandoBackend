using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Shared.BaseClasses
{
    public abstract class Entity<TId>
    {
        public TId Id { get; protected set; } = default!;

        public DateTime CreatedAt { get; protected set; }

        public DateTime? UpdatedAt { get; protected set; }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<TId> other)
                return false;

            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TId>.Default.GetHashCode(Id!);
        }
    }
}
