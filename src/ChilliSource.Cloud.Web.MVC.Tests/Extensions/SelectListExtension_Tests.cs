using ChilliSource.Core.Extensions;

namespace ChilliSource.Cloud.Web.MVC.Tests
{
    public enum TestEnum
    {
        Jim, John, Frank, Sam, Harry, [Obsolete]Sue
    }

    public class SelectListExtension_Tests
    {
        public class TestCollection
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private List<TestCollection> _collectionData()
        {
            return new List<TestCollection> { new TestCollection { Id = 1, Name = "one" }, new TestCollection { Id = 2, Name = "two" }, new TestCollection { Id = 3, Name = "three" }, new TestCollection { Id = 4, Name = "four" }, new TestCollection { Id = 4, Name = null } };
        }

        [Fact]
        public void Test_Func_Methods()
        {
            var collection = _collectionData();

            var selectlist = collection.ToSelectList(v => v.Id, t => t.Name, 2);

            Assert.Equal(2, selectlist.SelectedValue);
            Assert.Equal(_collectionData()[0].Name, selectlist.First().Text);
            Assert.Equal(String.Empty, selectlist.Last().Text);
            Assert.Equal(5, selectlist.Count());

            var multiselectlist = collection.ToSelectList(v => v.Id, t => t.Name, new List<int> { 2,3 });

            Assert.Contains(2, multiselectlist.SelectedValues.Cast<int>());
            Assert.Contains(3, multiselectlist.SelectedValues.Cast<int>());
            Assert.DoesNotContain(1, multiselectlist.SelectedValues.Cast<int>());
            Assert.Equal(5, multiselectlist.Count());

            var stringList = new List<string> { "1", "2", "3" };
            var stringSelectList = stringList.ToSelectList();
            Assert.Null(stringSelectList.SelectedValue);

            for(var i = 0; i < stringList.Count; i++)
            {
                Assert.Equal(stringList[i], stringSelectList.Skip(i).First().Value);
            }

            var intList = new List<int> { 1, 2, 3 };
            var intSelectList = intList.ToSelectList();
            Assert.Null(intSelectList.SelectedValue);

            for (var i = 0; i < intList.Count; i++)
            {
                Assert.Equal(intList[i], int.Parse(intSelectList.Skip(i).First().Value));
            }

            var enumList = EnumHelper.ToList<TestEnum>();
            var enumSelectList = enumList.ToSelectList();
            Assert.Null(enumSelectList.SelectedValue);
            Assert.Equal(enumList.Count, enumSelectList.Count());

            for (var i = 0; i < enumList.Count; i++)
            {
                Assert.Equal(enumList[i], EnumHelper.Parse<TestEnum>(enumSelectList.Skip(i).First().Value));
            }
        }

    }
}
