﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Jint.Bound
{
    partial class BoundTypeManager
    {
        public class DefiniteAssignmentMarker : IDisposable
        {
            private readonly BoundTypeManager _typeManager;
            private readonly Branch _parentBranch;
            private readonly HashSet<IBoundType> _unassignedWrites = new HashSet<IBoundType>();
            private bool _disposed;

            public DefiniteAssignmentMarker(BoundTypeManager typeManager, Branch parentBranch)
            {
                _typeManager = typeManager;
                _parentBranch = parentBranch;
            }

            public Branch CreateDefaultBranch()
            {
                return new Branch(this, null);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    // Process the unassigned writes. What we're doing here is
                    // that we go over all variables that are managed by the
                    // type manager, and marking all variables that do not
                    // appear in the unassigned writes list as being definitely
                    // assigned.

                    foreach (var type in _typeManager._types)
                    {
                        if (!_unassignedWrites.Contains(type))
                            ((BoundType)type).DefinitelyAssigned = true;
                    }

                    _disposed = true;
                }
            }

            public class Branch
            {
                private readonly DefiniteAssignmentMarker _manager;
                private readonly Branch _baseBranch;
                // We specifically use a list here and not a hash set because
                // we expect a small number of variables in this list and
                // HashSet has a considerable memory overhead.
                private List<BoundVariable> _assigned;
                // The last expression in the branch.

                public bool IsKilled { get; set; }
                public BoundExpression[] Expressions { get; private set; }

                public Branch(DefiniteAssignmentMarker manager, Branch baseBranch)
                {
                    _manager = manager;
                    _baseBranch = baseBranch;
                }

                public Branch Fork()
                {
                    return new Branch(_manager, this);
                }

                public void Join(IList<Branch> branches)
                {
                    // If there aren't any branches, there is no work.

                    if (branches.Count == 0)
                        return;

                    // If we just have a single branch, we can just copy all
                    // assignments.

                    if (branches.Count == 1)
                    {
                        var branch = branches[0];
                        if (branch._assigned != null)
                            MergeAssigned(branch._assigned);

                        // And move the expressions.
                        Expressions = branch.Expressions;

                        return;
                    }

                    // If we don't have a single branch, we have to merge
                    // the expressions from all branches.

                    BoundExpression[] expressions = null;
                    foreach (var branch in branches)
                    {
                        if (expressions == null)
                        {
                            // If we don't have expressions yet, just move them
                            // from the branch.

                            expressions = branch.Expressions;
                        }
                        else if (branch.Expressions != null)
                        {
                            // Otherwise, concatenate the two arrays.

                            var a = expressions;
                            var b = branch.Expressions;

                            var result = new BoundExpression[a.Length + b.Length];
                            Array.Copy(a, result, a.Length);
                            Array.Copy(b, 0, result, a.Length, b.Length);
                            expressions = result;
                        }
                    }

                    Expressions = expressions;

                    // If we have an empty branch, we don't have to do
                    // any work, because nothing will be definitely assigned.

                    if (branches.Any(p => p._assigned == null))
                        return;

                    // Otherwise, we need to get the intersection of all
                    // branches.

                    var assigned = new List<BoundVariable>(branches[0]._assigned);

                    for (int i = 1; i < branches.Count; i++)
                    {
                        var branchAssigned = branches[i]._assigned;

                        for (int j = assigned.Count - 1; j >= 0; j--)
                        {
                            // If the branch does not contain an assignment
                            // for the variable in our current collection,
                            // remove it from our current collection.

                            if (!branchAssigned.Contains(assigned[j]))
                                assigned.RemoveAt(j);
                        }

                        // Stop processing when we don't have any variables
                        // left.

                        if (assigned.Count == 0)
                            break;
                    }

                    // Add the assigned variables to our list.

                    if (assigned.Count > 0)
                        MergeAssigned(assigned);
                }

                private void MergeAssigned(List<BoundVariable> assigned)
                {
                    Debug.Assert(assigned != null && assigned.Count > 0);

                    if (_assigned == null)
                        _assigned = assigned;
                    else
                        _assigned.AddRange(assigned);
                }

                public void MarkRead(BoundVariable variable)
                {
                    // Determine whether the variable we're reading from has been
                    // definitely assigned. We need to look at our own list and
                    // all base branches to figure out whether the variable has
                    // actually been assigned to.

                    var branch = this;

                    while (branch != null)
                    {
                        // If we have an assignment, we're fine.

                        if (branch._assigned != null && branch._assigned.Contains(variable))
                            return;

                        // Look at the base branch.

                        branch = branch._baseBranch;
                    }

                    // If this is a closed over variable, we may need to go to the
                    // parent branch and handle the read there.

                    if (
                        variable.Type.Kind == BoundTypeKind.ClosureField &&
                        !_manager._typeManager._types.Contains(variable.Type)
                    )
                    {
                        Debug.Assert(_manager._parentBranch != null);

                        _manager._parentBranch.MarkRead(variable);
                    }
                    else
                    {
                        // If we didn't find an assignment, we have an unassigned
                        // variable. We do two things. First we add it to the list we're
                        // keeping of these variables, and we're marking it as written
                        // to. The reason for this is that we're actually reading an
                        // implicitly assigned 'undefined', so we can create an optimization
                        // here to stop further marking of the variable as being
                        // an unassigned write.

                        _manager._unassignedWrites.Add(variable.Type);

                        MarkWrite(variable);
                    }
                }

                public void MarkWrite(BoundVariable variable)
                {
                    if (_assigned == null)
                        _assigned = new List<BoundVariable>();

                    _assigned.Add(variable);
                }

                public void MarkExpression(BoundExpression expression)
                {
                    Expressions = new[] { expression };
                }
            }
        }
    }
}
