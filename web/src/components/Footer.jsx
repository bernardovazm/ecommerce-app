const Footer = () => {
  return (
    <footer className="bg-gray-800 text-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div>
            <h3 className="text-lg font-semibold mb-4">Ecommerce App</h3>
            <p className="text-gray-300 text-sm">Sua loja online</p>
          </div>
          <div>
            <h4 className="text-md font-semibold mb-4">Navegação</h4>
            <ul className="space-y-2 text-sm text-gray-300">
              <li>
                <a href="/" className="hover:text-white transition-colors">
                  Início
                </a>
              </li>
              <li>
                <a
                  href="/products"
                  className="hover:text-white transition-colors"
                >
                  Produtos
                </a>
              </li>
              <li>
                <a href="/cart" className="hover:text-white transition-colors">
                  Carrinho
                </a>
              </li>
            </ul>
          </div>
          <div>
            <h4 className="text-md font-semibold mb-4">
              Atendimento ao Cliente
            </h4>
            <ul className="space-y-2 text-sm text-gray-300">
              <li>
                <a href="/help" className="hover:text-white transition-colors">
                  Central de Ajuda
                </a>
              </li>
            </ul>
          </div>
          <div>
            <h4 className="text-md font-semibold mb-4">Redes Sociais</h4>
            <div className="flex space-x-4">
              <a
                href="#"
                className="text-gray-300 hover:text-white transition-colors"
              >
                Siga-nos
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
