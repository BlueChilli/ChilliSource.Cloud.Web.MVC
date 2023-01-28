namespace ChilliSource.Cloud.Web.MVC.Tests
{
    public class SelectListExtension_Tests
    {
        public class TestCollection
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private List<TestCollection> _collectionData()
        {
            return new List<TestCollection> { new TestCollection { Id = 1, Name = "one" }, new TestCollection { Id = 2, Name = "two" }, new TestCollection { Id = 3, Name = "three" }, new TestCollection { Id = 4, Name = "four" } };
        }

        [Fact]
        public void Test_Func_Methods()
        {
            var collection = _collectionData();

            var selectlist = collection.ToSelectList(v => v.Id, t => t.Name, 2);

            Assert.Equal(2, selectlist.SelectedValue);
            Assert.Equal(4, selectlist.Count());

            var multiselectlist = collection.ToSelectList(v => v.Id, t => t.Name, new List<int> { 2,3 });

            Assert.Contains(2, multiselectlist.SelectedValues.Cast<int>());
            Assert.Contains(3, multiselectlist.SelectedValues.Cast<int>());
            Assert.DoesNotContain(1, multiselectlist.SelectedValues.Cast<int>());
            Assert.Equal(4, multiselectlist.Count());

        }

    }
}
