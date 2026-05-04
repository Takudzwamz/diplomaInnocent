

using System;
using System.Linq.Expressions;
using Core.Interfaces;

namespace Core.Specifications;

public class BaseSpecification<T> : ISpecification<T>
{
    // Primary constructor
    public BaseSpecification(Expression<Func<T, bool>>? criteria)
    {
        Criteria = criteria;
    }

    // Parameterless constructor
    public BaseSpecification() : this(null) { }
    
    // --- NEW CONSTRUCTOR ADDED ---
    // Constructor for filtering and ordering in one step
   public BaseSpecification(Expression<Func<T, bool>> criteria, Expression<Func<T, object>> orderBy)
        : this(criteria) // <-- THIS IS THE FIX (changed from 'base' to 'this')
    {
        AddOrderBy(orderBy);
    }
    // --- END OF ADDITION ---

    public Expression<Func<T, bool>>? Criteria { get; }
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = []; // For ThenInclude
    public bool IsDistinct { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    public IQueryable<T> ApplyCriteria(IQueryable<T> query)
    {
        if (Criteria != null)
        {
            query = query.Where(Criteria);
        }
        return query;
    }

    public void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    public void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString); // For ThenInclude
    }

    public void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression ?? throw new ArgumentNullException(nameof(orderByExpression));
    }

    public void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression ?? throw new ArgumentNullException(nameof(orderByDescExpression));
    }

    public void ApplyDistinct()
    {
        IsDistinct = true;
    }

    public void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}

public class BaseSpecification<T, TResult> : BaseSpecification<T>, ISpecification<T, TResult>
{
    public BaseSpecification(Expression<Func<T, bool>>? criteria) : base(criteria) { }
    
    protected BaseSpecification() : this(null) { }
    
    public Expression<Func<T, TResult>>? Select { get; private set; }

    protected void AddSelect(Expression<Func<T, TResult>> selectExpression)
    {
        Select = selectExpression;
    }
}