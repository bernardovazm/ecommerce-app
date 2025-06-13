import { useEffect, useState, useCallback } from "react";
import { useSearchParams } from "react-router-dom";
import { Search, Filter } from "lucide-react";
import ProductCard from "../components/ProductCard";
import { productService, categoryService } from "../services/api";

const ProductListPage = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState(
    searchParams.get("search") || ""
  );
  const [selectedCategory, setSelectedCategory] = useState(
    searchParams.get("category") || ""
  );
  const [showFilters, setShowFilters] = useState(false);

  const pageSize = 12;

  useEffect(() => {
    fetchCategories();
  }, []);
  const fetchProducts = useCallback(async () => {
    setLoading(true);
    try {
      const response = await productService.search(
        searchTerm,
        selectedCategory,
        currentPage,
        pageSize
      );
      setProducts(response.data.products);
      setTotalCount(response.data.totalCount);
    } catch (error) {
      alert("Error fetching products:" + error);
    } finally {
      setLoading(false);
    }
  }, [searchTerm, selectedCategory, currentPage, pageSize]);

  useEffect(() => {
    fetchProducts();
  }, [currentPage, selectedCategory, searchTerm, fetchProducts]);

  const fetchCategories = async () => {
    try {
      const response = await categoryService.getAll();
      setCategories(response.data);
    } catch (error) {
      alert("Error fetching categories:" + error);
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setCurrentPage(1);
    setSearchParams({
      ...(searchTerm && { search: searchTerm }),
      ...(selectedCategory && { category: selectedCategory }),
    });
  };

  const handleCategoryChange = (category) => {
    setSelectedCategory(category);
    setCurrentPage(1);
    setSearchParams({
      ...(searchTerm && { search: searchTerm }),
      ...(category && { category }),
    });
  };

  const totalPages = Math.ceil(totalCount / pageSize);
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Produtos</h1>
        <div className="flex flex-col lg:flex-row gap-4">
          <form onSubmit={handleSearch} className="flex-1">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Buscar produtos..."
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </form>

          <button
            onClick={() => setShowFilters(!showFilters)}
            className="lg:hidden flex items-center justify-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
          >
            <Filter className="h-5 w-5 mr-2" />
            Filtros
          </button>
        </div>
      </div>
      <div className="flex flex-col lg:flex-row gap-8">
        <aside
          className={`lg:w-64 ${showFilters ? "block" : "hidden lg:block"}`}
        >
          <div className="bg-white rounded-lg shadow-sm p-6">
            <h3 className="text-lg font-semibold mb-4">Categorias</h3>
            <div className="space-y-2">
              <button
                onClick={() => handleCategoryChange("")}
                className={`block w-full text-left px-3 py-2 rounded-lg transition-colors ${!selectedCategory
                  ? "bg-blue-100 text-blue-700"
                  : "hover:bg-gray-100"
                  }`}
              >
                Todas as Categorias
              </button>
              {categories.map((category) => (
                <button
                  key={category.id}
                  onClick={() => handleCategoryChange(category.name)}
                  className={`block w-full text-left px-3 py-2 rounded-lg transition-colors ${selectedCategory === category.name
                    ? "bg-blue-100 text-blue-700"
                    : "hover:bg-gray-100"
                    }`}
                >
                  {category.name}
                </button>
              ))}
            </div>
          </div>
        </aside>
        <main className="flex-1">
          <div className="flex justify-between items-center mb-6">
            <p className="text-gray-600">
              Mostrando {(currentPage - 1) * pageSize + 1}-
              {Math.min(currentPage * pageSize, totalCount)} de {totalCount}{" "}
              produtos
            </p>
          </div>
          {loading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {[...Array(8)].map((_, i) => (
                <div
                  key={i}
                  className="bg-gray-200 animate-pulse rounded-lg h-80"
                ></div>
              ))}
            </div>
          ) : products.length > 0 ? (
            <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
              {products.map((product) => (
                <ProductCard key={product.id} product={product} />
              ))}
            </div>
          ) : (
            <div className="text-center py-12">
              <p className="text-gray-500 text-lg">Nenhum produto encontrado</p>
            </div>
          )}
          {totalPages > 1 && (
            <div className="flex justify-center mt-12">
              <nav className="flex space-x-2">
                {currentPage > 1 && (
                  <button
                    onClick={() => setCurrentPage(currentPage - 1)}
                    className="px-3 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                  >
                    Anterior
                  </button>
                )}

                {[...Array(totalPages)].map((_, i) => {
                  const page = i + 1;
                  if (
                    page === 1 ||
                    page === totalPages ||
                    (page >= currentPage - 2 && page <= currentPage + 2)
                  ) {
                    return (
                      <button
                        key={page}
                        onClick={() => setCurrentPage(page)}
                        className={`px-3 py-2 border rounded-lg ${currentPage === page
                          ? "bg-blue-600 text-white border-blue-600"
                          : "border-gray-300 hover:bg-gray-50"
                          }`}
                      >
                        {page}
                      </button>
                    );
                  }
                  return null;
                })}

                {currentPage < totalPages && (
                  <button
                    onClick={() => setCurrentPage(currentPage + 1)}
                    className="px-3 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                  >
                    Próximo
                  </button>
                )}
              </nav>
            </div>
          )}
        </main>
      </div>
    </div>
  );
};

export default ProductListPage;
