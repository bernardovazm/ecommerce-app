import PropTypes from 'prop-types';

const EmptyCartMessage = ({ navigate }) => (
    <div className="container mx-auto px-4 py-8">
        <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">
                Seu Carrinho está Vazio
            </h2>
            <p className="text-gray-600 mb-6">
                Adicione alguns itens ao seu carrinho antes de finalizar a compra.
            </p>
            <button
                onClick={() => navigate("/products")}
                className="bg-blue-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-blue-700 transition-colors"
            >
                Continuar Comprando
            </button>
        </div>
    </div>
);

EmptyCartMessage.propTypes = {
    navigate: PropTypes.func.isRequired
};

export default EmptyCartMessage;
