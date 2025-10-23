using System.Collections;
using System.Reflection;

namespace ConsoleCRUD.Services.Read
{
    //Extracts attributes from a type and builds a column model for tabular display. Uses TypeValueHelpers for type analysis and formatting.
    internal class ColumnModel
    {
        public static List<Column> BuildColumns(Type rootType, int maxDepth)
        {
            var cols = new List<Column>();
            var seen = new HashSet<Type>();

            FlattenType(rootType, path: null, depth: maxDepth, cols, seen, accessor: obj => obj);
            return cols;
        }

        private static void FlattenType(Type type,string? path,int depth,List<Column> cols, HashSet<Type> seen,Func<object, object?> accessor )
        {
            if (depth < 0) return;

            foreach (var m in ReadableMembers(type))
            {
                var header = string.IsNullOrEmpty(path) ? m.Name : $"{path}.{m.Name}";

                Func<object, object?> childAcc = obj =>
                {
                    var cur = accessor(obj);
                    try { return cur == null ? null : m.Getter(cur); }
                    catch { return null; }
                };

                if (TypeValueHelpers.IsPrimitiveLike(m.Type))
                {
                    cols.Add(new Column
                    {
                        Header = header,
                        Getter = obj => TypeValueHelpers.FormatPrimitive(childAcc(obj)),
                        AlignRight = TypeValueHelpers.IsNumericLike(m.Type)
                    });
                }
                else if (TypeValueHelpers.IsEnumerableButNotString(m.Type))
                {
                    cols.Add(new Column
                    {
                        Header = header + "[]",
                        Getter = obj => TypeValueHelpers.SummarizeEnumerable(childAcc(obj) as IEnumerable),
                        AlignRight = false
                    });
                }
                else
                {
                    if (seen.Contains(m.Type))
                    {
                        cols.Add(new Column { Header = header, Getter = _ => "(cycle)" });
                        continue;
                    }
                    seen.Add(m.Type);

                    FlattenType(m.Type, header, depth - 1, cols, seen, childAcc);
                    seen.Remove(m.Type);
                }
            }
        }

        private static IEnumerable<Member> ReadableMembers(Type t)
        {
            foreach (var property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead) continue;
                var propertyType = TypeValueHelpers.Underlying(property.PropertyType);
                yield return new Member
                {
                    Name = property.Name,
                    Type = propertyType,
                    Getter = (obj) => { try { return property.GetValue(obj); } catch { return null; } }
                };
            }

            foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var fieldType = TypeValueHelpers.Underlying(field.FieldType);
                yield return new Member
                {
                    Name = field.Name,
                    Type = fieldType,
                    Getter = (obj) => { try { return field.GetValue(obj); } catch { return null; } }
                };
            }
        }

        public static List<string[]> Materialize(List<object> rows, List<Column> cols)
        {
            var data = new List<string[]>(rows.Count);
            foreach (var r in rows)
            {
                var arr = new string[cols.Count];
                for (int i = 0; i < cols.Count; i++)
                {
                    try { arr[i] = cols[i].Getter(r) ?? ""; }
                    catch { arr[i] = ""; }
                }
                data.Add(arr);
            }
            return data;
        }
    }
}
