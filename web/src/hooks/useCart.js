import { useContext } from "react";
import { CartContext } from "../context/CartContextDefinition";

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error("useCart deve ser usado dentro de um CartProvider");
  }
  return context;
};
